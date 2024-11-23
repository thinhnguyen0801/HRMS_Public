using Dapper;
using HNOne.API.Constants;
using HNOne.Common;
using HNOne.Model.Models;
using HNOne.Model;
using System.Data;
using HNOne.Model.Entities;
using Microsoft.EntityFrameworkCore;
using HNOne.API.Repositories.Interfaces;
using Azure;
using static Dapper.SqlMapper;

namespace HNOne.API.Repositories
{
    public class ApprovalRepository : IApprovalRepository
    {
        private readonly MasterDbContext _dbContext;
        private readonly IDapperDbContext _dapperDbContext;
        private readonly IDateTimeHelper _dateTimeHelper;

        public ApprovalRepository(MasterDbContext dbContext
            , IDapperDbContext dapperDbContext, IDateTimeHelper dateTimeHelper)
        {
            _dbContext = dbContext;
            _dapperDbContext = dapperDbContext;
            _dateTimeHelper = dateTimeHelper;
        }

        #region Query

        /// <summary>
        /// lấy danh sách dữ liệu phê duyệt
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ApprovalModel>> GetApproval(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@EmployeeId", request.employeeId, DbType.Int32);
                parameters.Add("@Type", request.type, DbType.String);
                parameters.Add("@FromDate", request.fromDate, DbType.Date);
                parameters.Add("@ToDate", request.toDate, DbType.Date);
                var results = await connection.QueryAsync<ApprovalModel>(StoreConstants.STORE_H1_APPROVAL_SELECT, parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return results;
            }
        }

        /// <summary>
        /// lấy thông tin lịch sử chứng từ
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseModel> GetFnDocumentHistory(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                ResponseModel response = new ResponseModel();
                var parameters = new DynamicParameters();
                parameters.Add("@ObjType", request.type, DbType.String);
                parameters.Add("@DocEntry", request.documentId, DbType.Int32);
                parameters.Add("@Opt", $"{request.opt}", DbType.String);
                parameters.Add("@Opt1", $"{request.opt1}", DbType.String);
                string commandText = @$"select {StoreConstants.FUNC_GET_DOCUMENT_HISTORY}(@ObjType, @DocEntry, @Opt, @Opt1)";
                string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                if (string.IsNullOrEmpty(voucherNo))
                {
                    response.status = StatusCodes.Status204NoContent;
                    response.message = MessageConstants.MESSAGE_DOCUMENT_HISTORY_MISSING;
                    return response;
                }
                response.data = voucherNo;
                return response;
            }    
        }
        #endregion

        #region Command

        /// <summary>
        /// gửi phê duyệt
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddApproval(Approvals entity)
        {
            bool isTran = false;
            ResponseModel response = new ResponseModel();
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                if(entity.DocEntry < 1)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                string? voucherNo = string.Empty;
                // cập nhật tình trạng chứng từ
                switch(entity.ObjType)
                {
                    case GlobalConstants.TABLE_CONTRACT:
                        var contract = await _dbContext.Contracts.FirstOrDefaultAsync(m => m.Id == entity.DocEntry);
                        if(contract == null)
                        {
                            response.status = StatusCodes.Status404NotFound;
                            response.message = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Hợp đồng");
                            return response;
                        }
                        await _dbContext.Database.BeginTransactionAsync();
                        isTran = true;
                        voucherNo = contract.ContractCode;
                        contract.StatusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING; // ĐÃ GỬI YÊU CẦU PHÊ DUYỆT
                        contract.DateTracking = dateTimeNow;
                        _dbContext.Contracts.Attach(contract);
                        _dbContext.Entry(contract).State = EntityState.Modified;
                        break;
                    case GlobalConstants.TABLE_CONTRACT_APPENDIX:
                        var contractAppendix = await _dbContext.ContractAppendices.FirstOrDefaultAsync(m => m.Id == entity.DocEntry);
                        if (contractAppendix == null)
                        {
                            response.status = StatusCodes.Status404NotFound;
                            response.message = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Phụ lục hợp đồng");
                            return response;
                        }
                        await _dbContext.Database.BeginTransactionAsync();
                        isTran = true;
                        voucherNo = contractAppendix.ContractAppendixCode;
                        contractAppendix.StatusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING; // ĐÃ GỬI YÊU CẦU PHÊ DUYỆT
                        contractAppendix.DateTracking = dateTimeNow;
                        _dbContext.ContractAppendices.Attach(contractAppendix);
                        _dbContext.Entry(contractAppendix).State = EntityState.Modified;
                        break;
                    case GlobalConstants.TABLE_LEAVE_REQUEST:
                        var leaveRequest = await _dbContext.LeaveRequests.FirstOrDefaultAsync(m => m.Id == entity.DocEntry);
                        if (leaveRequest == null)
                        {
                            response.status = StatusCodes.Status404NotFound;
                            response.message = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Đề nghị nghỉ phép");
                            return response;
                        }
                        await _dbContext.Database.BeginTransactionAsync();
                        isTran = true;
                        voucherNo = leaveRequest.VoucherNo;
                        leaveRequest.StatusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING; // ĐÃ GỬI YÊU CẦU PHÊ DUYỆT
                        leaveRequest.DateTracking = dateTimeNow;
                        _dbContext.LeaveRequests.Attach(leaveRequest);
                        _dbContext.Entry(leaveRequest).State = EntityState.Modified;
                        break;
                    case GlobalConstants.TABLE_LEAVE_WORKING_HOURS:
                        var leaveWorkingHour = await _dbContext.LeaveWorkingHours.FirstOrDefaultAsync(m => m.Id == entity.DocEntry);
                        if (leaveWorkingHour == null)
                        {
                            response.status = StatusCodes.Status404NotFound;
                            response.message = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Xin nghỉ trong giờ");
                            return response;
                        }
                        await _dbContext.Database.BeginTransactionAsync();
                        isTran = true;
                        voucherNo = leaveWorkingHour.VoucherNo;
                        leaveWorkingHour.StatusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING; // ĐÃ GỬI YÊU CẦU PHÊ DUYỆT
                        leaveWorkingHour.DateTracking = dateTimeNow;
                        _dbContext.LeaveWorkingHours.Attach(leaveWorkingHour);
                        _dbContext.Entry(leaveWorkingHour).State = EntityState.Modified;
                        break;
                    case GlobalConstants.TABLE_SHIFT_CHANGE_REQUEST:
                        var shiftChangeleaveRequest = await _dbContext.ShiftChanges.FirstOrDefaultAsync(m => m.Id == entity.DocEntry);
                        if (shiftChangeleaveRequest == null)
                        {
                            response.status = StatusCodes.Status404NotFound;
                            response.message = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Đăng ký đổi ca");
                            return response;
                        }
                        await _dbContext.Database.BeginTransactionAsync();
                        isTran = true;
                        voucherNo = shiftChangeleaveRequest.VoucherNo;
                        shiftChangeleaveRequest.StatusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING; // ĐÃ GỬI YÊU CẦU PHÊ DUYỆT
                        shiftChangeleaveRequest.DateTracking = dateTimeNow;
                        _dbContext.ShiftChanges.Attach(shiftChangeleaveRequest);
                        _dbContext.Entry(shiftChangeleaveRequest).State = EntityState.Modified;
                        break;
                    case GlobalConstants.TABLE_OVERTIME_REQUEST:
                        var overtimeRequest = await _dbContext.OvertimeRequests.FirstOrDefaultAsync(m => m.Id == entity.DocEntry);
                        if (overtimeRequest == null)
                        {
                            response.status = StatusCodes.Status404NotFound;
                            response.message = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Đề nghị tăng ca");
                            return response;
                        }
                        await _dbContext.Database.BeginTransactionAsync();
                        isTran = true;
                        voucherNo = overtimeRequest.VoucherNo;
                        overtimeRequest.StatusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING; // ĐÃ GỬI YÊU CẦU PHÊ DUYỆT
                        overtimeRequest.DateTracking = dateTimeNow;
                        _dbContext.OvertimeRequests.Attach(overtimeRequest);
                        _dbContext.Entry(overtimeRequest).State = EntityState.Modified;
                        break;
                    default:
                        response.status = StatusCodes.Status404NotFound;
                        response.message = $"ObjType {entity.ObjType} was not provider!!!";
                        return response;
                }
                // Tạo mới
                entity.DateTracking = dateTimeNow;
                entity.CreateDate = dateTimeNow;
                await _dbContext.Approvals.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message = MessageConstants.MESSAGE_SEND_APPROVAL_SUCCESS;
                await createNotification(entity.DocEntry, entity.BranchId, entity.EmployeeSignatureId
                    , entity.UserSign ?? -1, voucherNo, entity.ObjType, CommonConstants.STATUS_CODE_APPROVAL_PENDING);
                return response;
            }
            catch (Exception ex)
            {
                if(isTran) await _dbContext.Database.RollbackTransactionAsync();
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// cập nhật tình trạng chứng từ
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateApproval(string actionType, IEnumerable<Approvals> lstEntity)
        {
            ResponseModel response = new ResponseModel();
            bool isTran = false;
            try
            {
                await _dbContext.Database.BeginTransactionAsync();
                isTran = true;
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                foreach (var entity in lstEntity)
                {
                    // cập nhật
                    var data = await _dbContext.Approvals.FirstOrDefaultAsync(m => m.Id == entity.Id);
                    if (data == null)
                    {
                        response.status = StatusCodes.Status404NotFound;
                        response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                        return response;
                    }
                    // dựa vào objtype đi kiếm phiếu và cập nhật tình trạng chứng từ
                    string? voucherNo = string.Empty;
                    int employeeId = -1;
                    switch (entity.ObjType)
                    {
                        case GlobalConstants.TABLE_CONTRACT:
                            var contract = await _dbContext.Contracts.FirstOrDefaultAsync(m => m.Id == entity.DocEntry);
                            if (contract == null)
                            {
                                response.status = StatusCodes.Status404NotFound;
                                response.message = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, $"Hợp đồng");
                                await _dbContext.Database.RollbackTransactionAsync(); // nếu không tìm thấy thì rollback hết
                                return response;
                            }
                            voucherNo = contract.ContractCode;
                            employeeId = contract.EmployeeId;
                            contract.StatusCode = entity.StatusCode; // tình trạng chứng từ "D": Đã duyệt, "T": từ chối, "C": đã hủy
                            contract.DateOfSigning = dateTimeNow; // cập nhật ngày ký
                            contract.DateTracking = dateTimeNow; // cập nhật ngày tracking
                            _dbContext.Contracts.Attach(contract);
                            _dbContext.Entry(contract).State = EntityState.Modified;
                            break;
                        case GlobalConstants.TABLE_CONTRACT_APPENDIX:
                            var contractAppendix = await _dbContext.ContractAppendices.FirstOrDefaultAsync(m => m.Id == entity.DocEntry);
                            if (contractAppendix == null)
                            {
                                response.status = StatusCodes.Status404NotFound;
                                response.message = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Phụ lục hợp đồng");
                                await _dbContext.Database.RollbackTransactionAsync(); // nếu không tìm thấy thì rollback hết
                                return response;
                            }
                            voucherNo = contractAppendix.ContractAppendixCode;
                            employeeId = contractAppendix.EmployeeId;
                            contractAppendix.StatusCode = entity.StatusCode; // tình trạng chứng từ "D": Đã duyệt, "T": từ chối, "C": đã hủy
                            contractAppendix.DateOfSigning = dateTimeNow; // cập nhật ngày ký
                            contractAppendix.DateTracking = dateTimeNow; // cập nhật ngày tracking
                            _dbContext.ContractAppendices.Attach(contractAppendix);
                            _dbContext.Entry(contractAppendix).State = EntityState.Modified;
                            break;
                        case GlobalConstants.TABLE_LEAVE_REQUEST:
                            var leaveRequest = await _dbContext.LeaveRequests.FirstOrDefaultAsync(m => m.Id == entity.DocEntry);
                            if (leaveRequest == null)
                            {
                                response.status = StatusCodes.Status404NotFound;
                                response.message = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Đề nghị nghỉ phép");
                                await _dbContext.Database.RollbackTransactionAsync(); // nếu không tìm thấy thì rollback hết
                                return response;
                            }
                            voucherNo = leaveRequest.VoucherNo;
                            employeeId = leaveRequest.EmployeeId;
                            leaveRequest.StatusCode = entity.StatusCode; // tình trạng chứng từ "D": Đã duyệt, "T": từ chối, "C": đã hủy
                            leaveRequest.DateOfSigning = dateTimeNow; // cập nhật ngày ký
                            leaveRequest.DateTracking = dateTimeNow; // cập nhật ngày tracking
                            _dbContext.LeaveRequests.Attach(leaveRequest);
                            _dbContext.Entry(leaveRequest).State = EntityState.Modified;
                            break;
                        case GlobalConstants.TABLE_LEAVE_WORKING_HOURS:
                            var leaveWorkingHour = await _dbContext.LeaveWorkingHours.FirstOrDefaultAsync(m => m.Id == entity.DocEntry);
                            if (leaveWorkingHour == null)
                            {
                                response.status = StatusCodes.Status404NotFound;
                                response.message = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Xin nghỉ trong giờ");
                                await _dbContext.Database.RollbackTransactionAsync(); // nếu không tìm thấy thì rollback hết
                                return response;
                            }
                            voucherNo = leaveWorkingHour.VoucherNo;
                            employeeId = leaveWorkingHour.EmployeeId;
                            leaveWorkingHour.StatusCode = entity.StatusCode; // tình trạng chứng từ "D": Đã duyệt, "T": từ chối, "C": đã hủy
                            leaveWorkingHour.DateOfSigning = dateTimeNow;
                            leaveWorkingHour.DateTracking = dateTimeNow; // cập nhật ngày tracking
                            _dbContext.LeaveWorkingHours.Attach(leaveWorkingHour);
                            _dbContext.Entry(leaveWorkingHour).State = EntityState.Modified;
                            break;
                        case GlobalConstants.TABLE_SHIFT_CHANGE_REQUEST:
                            var shiftChangeleaveRequest = await _dbContext.ShiftChanges.FirstOrDefaultAsync(m => m.Id == entity.DocEntry);
                            if (shiftChangeleaveRequest == null)
                            {
                                response.status = StatusCodes.Status404NotFound;
                                response.message = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Đăng ký đổi ca");
                                return response;
                            }
                            voucherNo = shiftChangeleaveRequest.VoucherNo;
                            employeeId = shiftChangeleaveRequest.EmployeeId;
                            shiftChangeleaveRequest.StatusCode = entity.StatusCode; // tình trạng chứng từ "D": Đã duyệt, "T": từ chối, "C": đã hủy
                            shiftChangeleaveRequest.DateOfSigning = dateTimeNow; // cập nhật ngày ký
                            shiftChangeleaveRequest.DateTracking = dateTimeNow; // cập nhật ngày tracking
                            _dbContext.ShiftChanges.Attach(shiftChangeleaveRequest);
                            _dbContext.Entry(shiftChangeleaveRequest).State = EntityState.Modified;
                            break;
                        case GlobalConstants.TABLE_OVERTIME_REQUEST:
                            var overtimeRequest = await _dbContext.OvertimeRequests.FirstOrDefaultAsync(m => m.Id == entity.DocEntry);
                            if (overtimeRequest == null)
                            {
                                response.status = StatusCodes.Status404NotFound;
                                response.message = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Đề nghị tăng ca");
                                return response;
                            }
                            voucherNo = overtimeRequest.VoucherNo;
                            employeeId = overtimeRequest.EmployeeId;
                            overtimeRequest.StatusCode = entity.StatusCode; // tình trạng chứng từ "D": Đã duyệt, "T": từ chối, "C": đã hủy
                            overtimeRequest.DateOfSigning = dateTimeNow; // cập nhật ngày ký
                            overtimeRequest.DateTracking = dateTimeNow; // cập nhật ngày tracking
                            _dbContext.OvertimeRequests.Attach(overtimeRequest);
                            _dbContext.Entry(overtimeRequest).State = EntityState.Modified;
                            break;
                        default:
                            response.status = StatusCodes.Status404NotFound;
                            response.message = $"ObjType {entity.ObjType} was not provider!!!";
                            await _dbContext.Database.RollbackTransactionAsync(); // nếu không tìm thấy thì rollback hết
                            return response;
                    }
                    data.StatusCode = entity.StatusCode;
                    data.ApprovalRemark = entity.ApprovalRemark?.Trim();
                    data.DateTracking = dateTimeNow;
                    data.UpdateDate = dateTimeNow;
                    data.UserSign2 = entity.UserSign2;
                    _dbContext.Approvals.Attach(data);
                    _dbContext.Entry(data).State = EntityState.Modified;

                    // lưu thông báo
                    await createNotification(entity.DocEntry, entity.BranchId, employeeId
                    , entity.UserSign2 ?? -1, voucherNo, entity.ObjType, entity.StatusCode);
                }    
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                if (actionType == CommonConstants.STATUS_CODE_APPROVED) response.message = MessageConstants.MESSAGE_APPROVAL_SUCCESS;
                else if (actionType == CommonConstants.STATUS_CODE_DENY) response.message = MessageConstants.MESSAGE_DENY_SUCCESS;
                else if (actionType == CommonConstants.STATUS_CODE_CANCELED) response.message = MessageConstants.MESSAGE_CANCEL_SUCCESS;
                else
                {
                }
            }
            catch (Exception ex)
            {
                if (isTran) await _dbContext.Database.RollbackTransactionAsync();
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;
        }
        #endregion

        #region Private Function

        /// <summary>
        /// lưu dữ liệu thông báo gửi đến ai
        /// </summary>
        /// <param name="docEntry"></param>
        /// <param name="branchId"></param>
        /// <param name="employeeId"></param>
        /// <param name="userId"></param>
        /// <param name="voucherNo"></param>
        /// <param name="objType"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        async Task createNotification(int docEntry, int branchId, int employeeId, int userId
            , string? voucherNo, string? objType, string? statusCode)
        {
            try
            {
                string message = string.Empty;
                if (statusCode == CommonConstants.STATUS_CODE_APPROVAL_PENDING) message = $"Bạn có chứng từ [{voucherNo}] đang chờ phê duyệt";
                else if (statusCode == CommonConstants.STATUS_CODE_APPROVED) message = $"Chứng từ [{voucherNo}] đã được phê duyệt";
                else if (statusCode == CommonConstants.STATUS_CODE_DENY) message = $"Chứng từ [{voucherNo}] đã bị từ chối";
                else if (statusCode == CommonConstants.STATUS_CODE_CANCELED) message = $"Chứng từ [{voucherNo}] đã bị hủy/trả về để chỉnh sữa";
                
                //
                if (string.IsNullOrEmpty(message)) return;
                Notifications entity = new Notifications();
                entity.Id = 0;
                entity.BranchId = branchId;
                entity.DocEntry = docEntry;
                entity.VoucherNo = voucherNo;
                entity.EmployeeId = employeeId;
                entity.ObjType = objType;
                entity.StatusCode = statusCode;
                entity.IsView = false;
                entity.Message = message;
                entity.UserSign = userId;
                entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                await _dbContext.Notifications.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
            }
            catch(Exception){}
        }
        #endregion
    }
}
