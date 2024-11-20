using HNOne.API.Repositories.Interfaces;
using HNOne.Common;
using HNOne.Model.Entities;
using HNOne.Model;
using Microsoft.EntityFrameworkCore;
using HNOne.Model.Models;
using Dapper;
using HNOne.API.Constants;
using System.Data;
using Newtonsoft.Json;

namespace HNOne.API.Repositories
{
    /// <summary>
    /// công phép
    /// </summary>
    public class WorkforceRepository : IWorkforceRepository
    {
        private readonly MasterDbContext _dbContext;
        private readonly IDapperDbContext _dapperDbContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        public WorkforceRepository(MasterDbContext dbContext
            , IDapperDbContext dapperDbContext, IDateTimeHelper dateTimeHelper)
        {
            _dbContext = dbContext;
            _dapperDbContext = dapperDbContext;
            _dateTimeHelper = dateTimeHelper;
        }

        #region Query
        /// <summary>
        /// Danh mục lấy thông tin cấu hình nghỉ phép
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<LeaveConfigModel>> GetLeaveConfig(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string query = "select T0.* from LeaveConfigs as T0 with(nolock) where T0.IsDelete = 0";
                var lstResult = await connection.QueryAsync<LeaveConfigModel>(query, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return lstResult;
            }    
        }
        
        /// <summary>
        /// lấy ra danh sách master dưới store trong phân hệ công phép
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<dynamic>> GetWorkforceMasterData(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@Type", request.type, DbType.String);
                parameters.Add("@Opt", $"{request.opt}", DbType.String);
                parameters.Add("@Opt1", $"{request.opt1}", DbType.String);
                parameters.Add("@Opt2", $"{request.opt2}", DbType.String);
                parameters.Add("@Opt3", $"{request.opt3}", DbType.String);
                var results = await connection.QueryAsync(StoreConstants.STORE_H1_WORKFORCE_MASTER_DATA_SELECT, parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return results;
            }
        }

        /// <summary>
        /// lấy danh sách đề nghị nghỉ phép
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<LeaveRequestModel>> GetLeaveRequest(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                request.fromDate ??= new DateTime(2000, 01, 01);
                request.toDate ??= DateTime.Now.AddMonths(1);
                var parameters = new DynamicParameters();
                parameters.Add("@LeaveRequestId", request.documentId, DbType.Int32);
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@StatusIds", request.opt, DbType.String);
                parameters.Add("@FromDate", request.fromDate, DbType.Date);
                parameters.Add("@ToDate", request.toDate, DbType.Date);
                IEnumerable<LeaveRequestModel>? lstResult = null;
                var dtResult = await connection.QueryMultipleAsync(StoreConstants.STORE_H1_LEAVE_REQUEST_SELECT, param: parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                if (dtResult != null)
                {
                    lstResult = dtResult.Read<LeaveRequestModel>();
                    if (request.documentId > 0)
                    {
                        var lstDetail = dtResult.Read<LeaveRequest1Model>();
                        string jsonDetail = JsonConvert.SerializeObject(lstDetail);
                        lstResult = lstResult.Update(m => m.jsonDetail = jsonDetail);
                    }
                }
                return lstResult ?? new List<LeaveRequestModel>();
            }
        }

        /// <summary>
        /// lấy danh sách đề nghị nghỉ phép
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<LeaveRequestModel>> GetLeaveWorkingHour(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                request.fromDate ??= new DateTime(2000, 01, 01);
                request.toDate ??= DateTime.Now.AddMonths(1);
                var parameters = new DynamicParameters();
                parameters.Add("@LeaveRequestId", request.documentId, DbType.Int32);
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@StatusIds", request.opt, DbType.String);
                parameters.Add("@FromDate", request.fromDate, DbType.Date);
                parameters.Add("@ToDate", request.toDate, DbType.Date);
                IEnumerable<LeaveRequestModel>? lstResult = null;
                var dtResult = await connection.QueryMultipleAsync(StoreConstants.STORE_H1_LEAVE_WORKING_HOUR_SELECT, param: parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                if (dtResult != null)
                {
                    lstResult = dtResult.Read<LeaveRequestModel>();
                    //if (request.documentId > 0)
                    //{
                    //    var lstDetail = dtResult.Read<LeaveRequest1Model>();
                    //    string jsonDetail = JsonConvert.SerializeObject(lstDetail);
                    //    lstResult = lstResult.Update(m => m.jsonDetail = jsonDetail);
                    //}
                }
                return lstResult ?? new List<LeaveRequestModel>();
            }
        }

        /// <summary>
        /// lấy danh mục ngày nghỉ lễ trong năm
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<HolidayCatagoryModel>> GetHolidayCatagory(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string query = "select T0.*" +
                    " ,T1.[Name] as TypeName" +
                    " from HolidayCatagories as T0 with(nolock)" +
                    " inner join dbo.HRM_FN_GET_ENUM('LoaiNgayNghi', '', '' ) as T1 on T0.Type = T1.Code" +
                    " where T0.IsDelete = 0";
                var lstResult = await connection.QueryAsync<HolidayCatagoryModel>(query, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return lstResult;
            }
        }
        #endregion

        #region Command

        /// <summary>
        /// cập nhật thông tin cấu hình phép
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateLeaveConfig(string actionType, LeaveConfigs entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                if (actionType == ProcessConstants.POST_LEAVE_CONFIG)
                {
                    // Tạo mới
                    entity.Id = await _dbContext.LeaveConfigs.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.DateTracking = dateTimeNow;
                    entity.CreateDate = dateTimeNow;
                    await _dbContext.LeaveConfigs.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    return response;
                }
                // cập nhật
                var data = await _dbContext.LeaveConfigs.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.Year = entity.Year;
                data.FromDate = entity.FromDate;
                data.ToDate = entity.ToDate;
                data.ExpiryDate = entity.ExpiryDate;
                data.AccrualDate = entity.AccrualDate;
                data.NumOfLeave = entity.NumOfLeave;
                data.NumOfYearIncrease = entity.NumOfYearIncrease;
                data.NumOfLeaveIncrease = entity.NumOfLeaveIncrease;
                data.NumOfLeaveTransfer = entity.NumOfLeaveTransfer;
                data.IsOffSaturday = entity.IsOffSaturday;
                data.IsOffSunday = entity.IsOffSunday;
                data.IsActive = entity.IsActive;
                data.DateTracking = dateTimeNow;
                data.UpdateDate = dateTimeNow;
                data.UserSign2 = entity.UserSign2;
                _dbContext.LeaveConfigs.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
            }
            catch (Exception ex)
            {
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;
        }
        
        /// <summary>
        /// Thêm mới chứng từ Đề nghị nghỉ phép
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lstEntity1"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddLeaveRequest(LeaveRequests entity, IEnumerable<LeaveRequest1s> lstEntity1)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@Type", GlobalConstants.TABLE_LEAVE_REQUEST, DbType.String);
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, '', '', '')";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    await _dbContext.Database.BeginTransactionAsync();
                    isTrans = true;
                    entity.Id = await _dbContext.LeaveRequests.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.VoucherNo = voucherNo;
                    entity.DateTracking = dateTimeNow;
                    entity.CreateDate = dateTimeNow;
                    await _dbContext.LeaveRequests.AddAsync(entity);
                    // thêm chi tiết đề nghị nghỉ phép
                    foreach(var item in lstEntity1)
                    {
                        LeaveRequest1s entity1 = new LeaveRequest1s();
                        entity1.LeaveRequestId = entity.Id;
                        entity1.DateOff = item.DateOff;
                        entity1.IsMorningBreak = item.IsMorningBreak;
                        entity1.IsAfternoonBreak = item.IsAfternoonBreak;
                        entity1.Remark = item.Remark;
                        entity1.DateTracking = dateTimeNow;
                        entity1.UserSign = entity.UserSign;
                        await _dbContext.LeaveRequest1s.AddAsync(entity1);
                    }    
                    await _dbContext.SaveChangesAsync();
                    await _dbContext.Database.CommitTransactionAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    response.data = entity.Id;
                }
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// cập nhật thông tin nghỉ phép
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lstEntity1"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateLeaveRequest(LeaveRequests entity, IEnumerable<LeaveRequest1s> lstEntity1)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.LeaveRequests.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                data.EmployeeId = entity.EmployeeId;
                data.EmployeeSignatureId = entity.EmployeeSignatureId;
                data.ReasonId = entity.ReasonId;
                data.DepartmentId = entity.DepartmentId;
                data.StatusCode = entity.StatusCode;
                data.FromDate = entity.FromDate;
                data.ToDate = entity.ToDate;
                data.Remark = entity.Remark;
                data.DateTracking = dateTimeNow;
                data.UpdateDate = dateTimeNow;
                data.UserSign2 = entity.UserSign2;
                await _dbContext.Database.BeginTransactionAsync();
                isTrans = true;
                _dbContext.LeaveRequests.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                // thêm chi tiết đề nghị nghỉ phép
                foreach (var item in lstEntity1)
                {
                    LeaveRequest1s entity1 = new LeaveRequest1s();
                    entity1.LeaveRequestId = entity.Id;
                    entity1.DateOff = item.DateOff;
                    entity1.IsMorningBreak = item.IsMorningBreak;
                    entity1.IsAfternoonBreak = item.IsAfternoonBreak;
                    entity1.Remark = item.Remark;
                    entity1.DateTracking = dateTimeNow;
                    entity1.UserSign = entity.UserSign;
                    await _dbContext.LeaveRequest1s.AddAsync(entity1);
                }
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                response.data = data.Id;
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }
        }
        
        /// <summary>
        /// lưu thông tin xin phép nghỉ trong giờ
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateLeaveWorkingHours(string actionType, LeaveWorkingHours entity)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                if (actionType == ProcessConstants.POST_LEAVE_WORKING_HOUR)
                {
                    using (var connection = _dapperDbContext.CreateConnection())
                    {
                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@Type", GlobalConstants.TABLE_LEAVE_WORKING_HOURS, DbType.String);
                        string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, '', '', '')";
                        string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                        if (string.IsNullOrEmpty(voucherNo))
                        {
                            response.status = StatusCodes.Status204NoContent;
                            response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                            return response;
                        }
                        entity.Id = await _dbContext.LeaveWorkingHours.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                        entity.VoucherNo = voucherNo;
                        entity.DateTracking = dateTimeNow;
                        entity.CreateDate = dateTimeNow;
                        await _dbContext.LeaveWorkingHours.AddAsync(entity);
                        await _dbContext.SaveChangesAsync();
                        response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                        response.data = entity.Id;
                    }
                }
                else
                {
                    var data = await _dbContext.LeaveWorkingHours.FirstOrDefaultAsync(m => m.Id == entity.Id);
                    if (data == null)
                    {
                        response.status = StatusCodes.Status404NotFound;
                        response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                        return response;
                    }
                    data.EmployeeId = entity.EmployeeId;
                    data.EmployeeSignatureId = entity.EmployeeSignatureId;
                    data.DepartmentId = entity.DepartmentId;
                    data.StatusCode = entity.StatusCode;
                    data.FromDate = entity.FromDate;
                    data.ToDate = entity.ToDate;
                    data.Remark = entity.Remark;
                    data.DateTracking = dateTimeNow;
                    data.UpdateDate = dateTimeNow;
                    data.UserSign2 = entity.UserSign2;
                    _dbContext.LeaveWorkingHours.Attach(data);
                    _dbContext.Entry(data).State = EntityState.Modified;
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                    response.data = data.Id;
                }
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }
        }
        
        /// <summary>
        /// cập nhật thông tin số ngày nghỉ lễ trong năm
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateHolidayCatagory(string actionType, HolidayCatagories entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                if (actionType == ProcessConstants.POST_HOILDAY_CATAGORY)
                {
                    // kiểm tra có ngày trùng không
                    //var checkExists = await _dbContext.HolidayCatagories.AnyAsync(m => (m.FromDate.Date <= entity.FromDate.Date && entity.FromDate.Date <= entity.ToDate.Date)
                    //    || (m.FromDate.Date <= entity.ToDate.Date && entity.ToDate.Date <= m.ToDate.Date));
                    var checkExists = await _dbContext.HolidayCatagories.AnyAsync(m => !(entity.ToDate.Date < m.FromDate.Date || entity.FromDate.Date > m.ToDate.Date));
                    if (checkExists)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = "Từ ngày hoặc đến ngày đã tồn tại trong hệ thống";
                        return response;
                    }    
                    // Tạo mới
                    entity.DateTracking = dateTimeNow;
                    entity.CreateDate = dateTimeNow;
                    await _dbContext.HolidayCatagories.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                    return response;
                }
                // cập nhật
                var data = await _dbContext.HolidayCatagories.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.Name = entity.Name;
                data.FromDate = entity.FromDate;
                data.ToDate = entity.ToDate;
                data.Color = entity.Color;
                data.Type = entity.Type;
                data.Remark = entity.Remark;
                data.DateTracking = dateTimeNow;
                data.UpdateDate = dateTimeNow;
                data.UserSign2 = entity.UserSign2;
                _dbContext.HolidayCatagories.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch(Exception) { throw; }
        }
        #endregion
    }
}
