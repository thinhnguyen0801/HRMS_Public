using Dapper;
using HNOne.API.Constants;
using HNOne.API.Repositories.Interfaces;
using HNOne.API.Services;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace HNOne.API.Repositories
{
    public class ApprovalRepository : IApprovalRepository
    {
        private readonly MasterDbContext _dbContext;
        private readonly IDapperDbContext _dapperDbContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IHubContext<HubService> _hubContext;

        public ApprovalRepository(MasterDbContext dbContext
            , IDapperDbContext dapperDbContext, IDateTimeHelper dateTimeHelper, IHubContext<HubService> hubContext)
        {
            _dbContext = dbContext;
            _dapperDbContext = dapperDbContext;
            _dateTimeHelper = dateTimeHelper;
            _hubContext = hubContext;
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

        #endregion Query

        #region Command

        /// <summary>
        /// gửi phê duyệt
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddApproval(ApprovalModel entityModel)
        {
            bool isTran = false;
            ResponseModel response = new ResponseModel();
            try
            {
                if (entityModel.docEntry < 1)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                // Ánh xạ ObjType tới các bảng tương ứng
                var entityHandlers = new Dictionary<string, Func<int, Task<(object? Entity, string? VoucherNo)>>>
                {
                    { GlobalConstants.TABLE_CONTRACT, async id => { var contract = await _dbContext.Contracts.FirstOrDefaultAsync(m => m.Id == id); return (contract, contract?.ContractCode); } },
                    { GlobalConstants.TABLE_CONTRACT_APPENDIX, async id => { var contractAppendix = await _dbContext.ContractAppendices.FirstOrDefaultAsync(m => m.Id == id); return (contractAppendix, contractAppendix?.ContractAppendixCode); } },
                    { GlobalConstants.TABLE_LEAVE_REQUEST, async id => { var leaveRequest = await _dbContext.LeaveRequests.FirstOrDefaultAsync(m => m.Id == id); return (leaveRequest, leaveRequest?.VoucherNo); } },
                    { GlobalConstants.TABLE_LEAVE_WORKING_HOURS, async id => { var leaveHours = await _dbContext.LeaveWorkingHours.FirstOrDefaultAsync(m => m.Id == id); return (leaveHours, leaveHours?.VoucherNo);} },
                    { GlobalConstants.TABLE_SHIFT_CHANGE_REQUEST, async id => { var shiftChange = await _dbContext.ShiftChanges.FirstOrDefaultAsync(m => m.Id == id);return (shiftChange, shiftChange?.VoucherNo);} },
                    { GlobalConstants.TABLE_OVERTIME_REQUEST, async id => { var overtime = await _dbContext.OvertimeRequests.FirstOrDefaultAsync(m => m.Id == id);return (overtime, overtime?.VoucherNo);} },
                    { GlobalConstants.TABLE_TRAINING, async id => { var training = await _dbContext.Trainings.FirstOrDefaultAsync(m => m.Id == id); return (training, training?.VoucherNo); } },
                    { GlobalConstants.TABLE_CONFIRM_WORKING_DAY, async id => { var confirm = await _dbContext.ConfirmWorkingDays.FirstOrDefaultAsync(m => m.Id == id); return (confirm, confirm?.VoucherNo); } },
                    { GlobalConstants.TABLE_SALARY_EXPENSE_ACCOUNTING, async id => { var salary = await _dbContext.SalaryExpenseAccountings.FirstOrDefaultAsync(m => m.Id == id); return (salary, salary?.VoucherNo); } }
                };
                if (!entityHandlers.TryGetValue($"{entityModel.objType}", out var handler))
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = $"ObjType {entityModel.objType} was not provided!";
                    return response;
                }
                var (record, voucherNo) = await handler(entityModel.docEntry);
                if (record == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, entityModel.objType);
                    return response;
                }
                await _dbContext.Database.BeginTransactionAsync();
                isTran = true;
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                dynamic dynamicRecord = record;

                // Thêm vào lịch sử
                await addDocumentHistory(dynamicRecord.BranchId, entityModel.employeeId, entityModel.docEntry, $"{entityModel.objType}"
                    , entityModel.statusCode, dynamicRecord.StatusCode, entityModel.approvalRemark, entityModel.userSign ?? -1, dateTimeNow);

                // cập nhật tình trạng chứng từ
                dynamicRecord.StatusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING; // ĐÃ GỬI YÊU CẦU PHÊ DUYỆT
                dynamicRecord.DateTracking = dateTimeNow; // ĐÃ GỬI YÊU CẦU PHÊ DUYỆT
                _dbContext.Attach(dynamicRecord);
                _dbContext.Entry(dynamicRecord).State = EntityState.Modified;

                // Tạo mới phê duyệt
                Approvals entity = new Approvals();
                entity.BranchId = entityModel.branchId;
                entity.DocEntry = entityModel.docEntry;
                entity.ObjType = entityModel.objType;
                entity.StatusCode = entityModel.statusCode;
                entity.EmployeeSignatureId = entityModel.employeeSignatureId;
                entity.ApprovalRemark = entityModel.approvalRemark;
                entity.DateTracking = dateTimeNow;
                entity.CreateDate = dateTimeNow;
                entity.UserSign = entityModel.userSign;
                await _dbContext.Approvals.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message = MessageConstants.MESSAGE_SEND_APPROVAL_SUCCESS;

                // gửi thông báo
                await createNotification(entity.DocEntry, entity.BranchId, entity.EmployeeSignatureId
                    , entity.UserSign ?? -1, voucherNo, entity.ObjType, CommonConstants.STATUS_CODE_APPROVAL_PENDING);
                return response;
            }
            catch (Exception ex)
            {
                if (isTran) await _dbContext.Database.RollbackTransactionAsync();
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// cập nhật tình trạng chứng từ + phê duyệt
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateApproval(string actionType, IEnumerable<ApprovalModel> lstEntity)
        {
            bool isTran = false;
            ResponseModel response = new ResponseModel();
            try
            {
                // Ánh xạ ObjType tới các bảng tương ứng
                var entityHandlers = new Dictionary<string, Func<int, Task<(object? Entity, string? VoucherNo, int? EmployeeId)>>>
                {
                    { GlobalConstants.TABLE_CONTRACT, async id => { var contract = await _dbContext.Contracts.FirstOrDefaultAsync(m => m.Id == id); return (contract, contract?.ContractCode, contract?.EmployeeId); } },
                    { GlobalConstants.TABLE_CONTRACT_APPENDIX, async id => { var contractAppendix = await _dbContext.ContractAppendices.FirstOrDefaultAsync(m => m.Id == id); return (contractAppendix, contractAppendix?.ContractAppendixCode, contractAppendix?.EmployeeId); } },
                    { GlobalConstants.TABLE_LEAVE_REQUEST, async id => { var leaveRequest = await _dbContext.LeaveRequests.FirstOrDefaultAsync(m => m.Id == id); return (leaveRequest, leaveRequest?.VoucherNo, leaveRequest?.EmployeeId); } },
                    { GlobalConstants.TABLE_LEAVE_WORKING_HOURS, async id => { var leaveHours = await _dbContext.LeaveWorkingHours.FirstOrDefaultAsync(m => m.Id == id); return (leaveHours, leaveHours?.VoucherNo, leaveHours?.EmployeeId);} },
                    { GlobalConstants.TABLE_SHIFT_CHANGE_REQUEST, async id => { var shiftChange = await _dbContext.ShiftChanges.FirstOrDefaultAsync(m => m.Id == id);return (shiftChange, shiftChange?.VoucherNo, shiftChange?.EmployeeId);} },
                    { GlobalConstants.TABLE_OVERTIME_REQUEST, async id => { var overtime = await _dbContext.OvertimeRequests.FirstOrDefaultAsync(m => m.Id == id);return (overtime, overtime?.VoucherNo, overtime?.EmployeeId);} },
                    { GlobalConstants.TABLE_TRAINING, async id => { var training = await _dbContext.Trainings.FirstOrDefaultAsync(m => m.Id == id); return (training, training?.VoucherNo, -1); } },
                    { GlobalConstants.TABLE_CONFIRM_WORKING_DAY, async id => { var confirm = await _dbContext.ConfirmWorkingDays.FirstOrDefaultAsync(m => m.Id == id); return (confirm, confirm?.VoucherNo, confirm?.EmployeeId); } },
                    { GlobalConstants.TABLE_SALARY_EXPENSE_ACCOUNTING, async id => { var salary = await _dbContext.SalaryExpenseAccountings.FirstOrDefaultAsync(m => m.Id == id); return (salary, salary?.VoucherNo, -1); } }
                };
                await _dbContext.Database.BeginTransactionAsync();
                isTran = true;
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                foreach (var entity in lstEntity)
                {
                    // cập nhật
                    var data = await _dbContext.Approvals.FirstOrDefaultAsync(m => m.Id == entity.id);
                    if (data == null)
                    {
                        response.status = StatusCodes.Status404NotFound;
                        response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                        if (isTran) await _dbContext.Database.RollbackTransactionAsync();
                        return response;
                    }
                    if (!entityHandlers.TryGetValue($"{entity.objType}", out var handler))
                    {
                        response.status = StatusCodes.Status404NotFound;
                        response.message = $"ObjType {entity.objType} was not provided!";
                        await _dbContext.Database.RollbackTransactionAsync();
                        return response;
                    }
                    var (record, voucherNo, employeeId) = await handler(entity.docEntry);
                    if (record == null)
                    {
                        response.status = StatusCodes.Status404NotFound;
                        response.message = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, entity.objType);
                        await _dbContext.Database.RollbackTransactionAsync();
                        return response;
                    }
                    dynamic dynamicRecord = record;
                    // Thêm vào lịch sử
                    await addDocumentHistory(dynamicRecord.BranchId, entity.employeeId, entity.docEntry, entity.objType
                        , entity.statusCode, dynamicRecord.StatusCode, entity.approvalRemark, entity.userSign ?? -1, dateTimeNow);

                    // cập nhật tình trạng chứng từ
                    dynamicRecord.StatusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING; // ĐÃ GỬI YÊU CẦU PHÊ DUYỆT
                    dynamicRecord.DateOfSigning = dateTimeNow;
                    dynamicRecord.DateTracking = dateTimeNow;
                    _dbContext.Attach(dynamicRecord);
                    _dbContext.Entry(dynamicRecord).State = EntityState.Modified;

                    // cập nhật tình trạng phê duyệt
                    data.StatusCode = entity.statusCode;
                    data.ApprovalRemark = entity.approvalRemark?.Trim();
                    data.DateTracking = dateTimeNow;
                    data.UpdateDate = dateTimeNow;
                    data.UserSign2 = entity.userSign2;
                    _dbContext.Approvals.Attach(data);
                    _dbContext.Entry(data).State = EntityState.Modified;

                    // cập nhật tình trạng
                    await updateStatusNotification(entity.docEntry, employeeId ?? -1, entity.objType);
                    // lưu thông báo
                    await createNotification(entity.docEntry, entity.branchId, employeeId ?? -1
                    , entity.userSign2 ?? -1, voucherNo, entity.objType, entity.statusCode);
                }
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message = actionType switch
                {
                    CommonConstants.STATUS_CODE_APPROVED => MessageConstants.MESSAGE_APPROVAL_SUCCESS,
                    CommonConstants.STATUS_CODE_DENY => MessageConstants.MESSAGE_DENY_SUCCESS,
                    CommonConstants.STATUS_CODE_CANCELED => MessageConstants.MESSAGE_CANCEL_SUCCESS,
                    _ => response.message
                };
            }
            catch (Exception ex)
            {
                if (isTran) await _dbContext.Database.RollbackTransactionAsync();
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Hủy chứng từ
        /// Áp dụng đổi với các chứng từ đang chở xử lý
        /// </summary>
        /// <param name="lstEntity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> CancelDocument(IEnumerable<ApprovalModel> lstEntity)
        {
            ResponseModel response = new ResponseModel();
            bool isTran = false;
            try
            {
                await _dbContext.Database.BeginTransactionAsync();
                isTran = true;
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                //Ánh xạ ObjType tới các bảng tương ứng
                var entityHandlers = new Dictionary<string, Func<int, Task<object?>>>
                {
                    { GlobalConstants.TABLE_CONTRACT, async id => await _dbContext.Contracts.FirstOrDefaultAsync(m => m.Id == id) },
                    { GlobalConstants.TABLE_CONTRACT_APPENDIX, async id => await _dbContext.ContractAppendices.FirstOrDefaultAsync(m => m.Id == id) },
                    { GlobalConstants.TABLE_LEAVE_REQUEST, async id => await _dbContext.LeaveRequests.FirstOrDefaultAsync(m => m.Id == id) },
                    { GlobalConstants.TABLE_LEAVE_WORKING_HOURS, async id => await _dbContext.LeaveWorkingHours.FirstOrDefaultAsync(m => m.Id == id) },
                    { GlobalConstants.TABLE_SHIFT_CHANGE_REQUEST, async id => await _dbContext.ShiftChanges.FirstOrDefaultAsync(m => m.Id == id) },
                    { GlobalConstants.TABLE_OVERTIME_REQUEST, async id => await _dbContext.OvertimeRequests.FirstOrDefaultAsync(m => m.Id == id) },
                    { GlobalConstants.TABLE_TRAINING, async id => await _dbContext.Trainings.FirstOrDefaultAsync(m => m.Id == id) },
                    { GlobalConstants.TABLE_CONFIRM_WORKING_DAY, async id => await _dbContext.ConfirmWorkingDays.FirstOrDefaultAsync(m => m.Id == id) }
                };
                foreach (var entity in lstEntity)
                {
                    if (!entityHandlers.TryGetValue($"{entity.objType}", out var handler))
                    {
                        response.status = StatusCodes.Status404NotFound;
                        response.message = $"ObjType {entity.objType} was not provided!";
                        await _dbContext.Database.RollbackTransactionAsync();
                        return response;
                    }
                    var record = await handler(entity.docEntry);
                    if (record == null)
                    {
                        response.status = StatusCodes.Status404NotFound;
                        response.message = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, entity.objType);
                        await _dbContext.Database.RollbackTransactionAsync();
                        return response;
                    }
                    dynamic dynamicRecord = record;
                    // thêm vào lịch sử chứng từ
                    await addDocumentHistory(dynamicRecord.BranchId, entity.employeeId, entity.docEntry, $"{entity.objType}"
                        , entity.statusCode, dynamicRecord.StatusCode, entity.approvalRemark, entity.userSign ?? -1, dateTimeNow);

                    // Cập nhật trạng thái và ngày tracking
                    dynamicRecord.StatusCode = entity.statusCode;
                    dynamicRecord.DateTracking = dateTimeNow;
                    _dbContext.Attach(dynamicRecord);
                    _dbContext.Entry(dynamicRecord).State = EntityState.Modified;
                }
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message = MessageConstants.MESSAGE_CANCEL_SUCCESS;
            }
            catch (Exception ex)
            {
                if (isTran) await _dbContext.Database.RollbackTransactionAsync();
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;
        }

        #endregion Command

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
        private async Task createNotification(int docEntry, int branchId, int employeeId, int userId
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
                // Lấy ConnectionId từ UserId
                var connectionId = HubService.GetConnectionIdByUserId(employeeId.ToString());
                if (connectionId != null)
                {
                    // Gửi thông báo đến ConnectionId
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// cập nhật tình trạng thông báo
        /// </summary>
        /// <param name="docEntry"></param>
        /// <param name="employeeId"></param>
        /// <param name="objType"></param>
        /// <returns></returns>
        private async Task updateStatusNotification(int docEntry, int employeeId, string? objType)
        {
            try
            {
                var entity = await _dbContext.Notifications.FirstOrDefaultAsync(m => m.ObjType == objType && m.EmployeeId == employeeId && m.DocEntry == docEntry && m.IsView == false);
                if (entity != null)
                {
                    entity.IsView = true;
                    _dbContext.Notifications.Attach(entity);
                    _dbContext.Entry(entity).State = EntityState.Modified;
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Thêm dữ liệu lịch sử chứng từ
        /// </summary>
        /// <param name="branchId"></param>
        /// <param name="employeeId"></param>
        /// <param name="docEntry"></param>
        /// <param name="objType"></param>
        /// <param name="statusCode"></param>
        /// <param name="statusCodePre"></param>
        /// <param name="remark"></param>
        /// <param name="userId"></param>
        /// <param name="createDate"></param>
        /// <returns></returns>
        private async Task addDocumentHistory(int branchId, int employeeId, int docEntry, string? objType
            , string? statusCode, string? statusCodePre, string remark, int userId, DateTime createDate)
        {
            try
            {
                DocumentHistories entity = new DocumentHistories();
                entity.BranchId = branchId;
                entity.EmployeeId = employeeId;
                entity.DocEntry = docEntry;
                entity.ObjType = objType;
                entity.StatusCode = statusCode;
                entity.StatusCodePre = statusCodePre;
                entity.Remark = remark;
                entity.UserSign = userId;
                entity.CreateDate = createDate;
                await _dbContext.DocumentHistories.AddAsync(entity);
            }
            catch (Exception) { throw; }
        }

        #endregion Private Function
    }
}