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
using Azure;
using System.Text.RegularExpressions;

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
                var parameters = new DynamicParameters();
                string strQuery = "select T0.* " +
                    ",T2.BranchCode, T2.BranchName" +
                    " from LeaveConfigs as T0 with(nolock) " +
                    " inner join Branchs as T2 with(nolock) on T0.BranchId = T2.BranchId" +
                    " where T0.IsDelete = 0";
                // thêm điều kiện
                if (request.opt == CommonConstants.ENUM_ACTIVE)
                {
                    // ở các chứng từ
                    strQuery += " and T0.IsActive = '1'";
                    strQuery += " and T0.BranchId = @BranchId";
                    parameters.Add("@BranchId", request.branchId, DbType.Int32);
                }
                else
                {
                    // Ở page danh mục. Kiểm tra có quyền xem chi nhánh nào thì where thêm
                    request.branchIds += $",{request.branchId}";
                    request.branchIds = request.branchIds.Trim(',');
                    strQuery += " and T0.BranchId in ((select [Value] from string_split(@BranchIds, ',')))";
                    parameters.Add("@BranchIds", request.branchIds, DbType.String);
                }
                var lstResult = await connection.QueryAsync<LeaveConfigModel>(strQuery, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
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
                parameters.Add("@EmployeeId", request.employeeId, DbType.Int32);
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
                parameters.Add("@EmployeeId", request.employeeId, DbType.Int32);
                parameters.Add("@BranchIds", $"{request.branchIds}", DbType.String);
                parameters.Add("@DepartmentIds", $"{request.departmentIds}", DbType.String);
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
                parameters.Add("@EmployeeId", request.employeeId, DbType.Int32);
                parameters.Add("@BranchIds", $"{request.branchIds}", DbType.String);
                parameters.Add("@DepartmentIds", $"{request.departmentIds}", DbType.String);
                IEnumerable<LeaveRequestModel>? lstResult = null;
                var dtResult = await connection.QueryMultipleAsync(StoreConstants.STORE_H1_LEAVE_WORKING_HOUR_SELECT, param: parameters
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
        /// lấy danh mục ngày nghỉ lễ trong năm
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<HolidayCatagoryModel>> GetHolidayCatagory(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                string strQuery = "select T0.*" +
                    " ,T1.[Name] as TypeName" +
                    " ,T2.BranchCode, T2.BranchName" +
                    " from HolidayCatagories as T0 with(nolock)" +
                    " inner join dbo.HRM_FN_GET_ENUM('LoaiNgayNghi', '', '' ) as T1 on T0.Type = T1.Code" +
                    " inner join Branchs as T2 with(nolock) on T0.BranchId = T2.BranchId" +
                    " where T0.IsDelete = 0";
                // thêm điều kiện
                if (request.opt == CommonConstants.ENUM_ACTIVE)
                {
                    // ở các chứng từ
                    strQuery += " and T0.IsActive = '1'";
                    strQuery += " and T0.BranchId = @BranchId";
                    parameters.Add("@BranchId", request.branchId, DbType.Int32);
                }
                else
                {
                    // Ở page danh mục. Kiểm tra có quyền xem chi nhánh nào thì where thêm
                    request.branchIds += $",{request.branchId}";
                    request.branchIds = request.branchIds.Trim(',');
                    strQuery += " and T0.BranchId in ((select [Value] from string_split(@BranchIds, ',')))";
                    parameters.Add("@BranchIds", request.branchIds, DbType.String);
                }
                var lstResult = await connection.QueryAsync<HolidayCatagoryModel>(strQuery, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return lstResult;
            }
        }

        /// <summary>
        /// lấy danh sách Đăng ký đổi ca
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ShiftChangeModel>> GetShiftChange(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                request.fromDate ??= new DateTime(2000, 01, 01);
                request.toDate ??= DateTime.Now.AddMonths(1);
                var parameters = new DynamicParameters();
                parameters.Add("@DocumentId", request.documentId, DbType.Int32);
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@StatusIds", request.opt, DbType.String);
                parameters.Add("@FromDate", request.fromDate, DbType.Date);
                parameters.Add("@ToDate", request.toDate, DbType.Date);
                parameters.Add("@EmployeeId", request.employeeId, DbType.Int32);
                parameters.Add("@BranchIds", $"{request.branchIds}", DbType.String);
                parameters.Add("@DepartmentIds", $"{request.departmentIds}", DbType.String);
                IEnumerable<ShiftChangeModel>? lstResult = null;
                var dtResult = await connection.QueryMultipleAsync(StoreConstants.STORE_H1_SHIFT_CHANGE_REQUEST_SELECT, param: parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                if (dtResult != null)
                {
                    lstResult = dtResult.Read<ShiftChangeModel>();
                    if (request.documentId > 0)
                    {
                        var lstDetail = dtResult.Read<ShiftChange1Model>();
                        string jsonDetail = JsonConvert.SerializeObject(lstDetail);
                        lstResult = lstResult.Update(m => m.jsonDetail = jsonDetail);
                    }
                }
                return lstResult ?? new List<ShiftChangeModel>();
            }
        }

        /// <summary>
        /// lấy danh sách Đăng ký đổi ca
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<OvertimeRequestModel>> GetOvertimeRequest(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                request.fromDate ??= new DateTime(2000, 01, 01);
                request.toDate ??= DateTime.Now.AddMonths(1);
                var parameters = new DynamicParameters();
                parameters.Add("@DocumentId", request.documentId, DbType.Int32);
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@StatusIds", request.opt, DbType.String);
                parameters.Add("@FromDate", request.fromDate, DbType.Date);
                parameters.Add("@ToDate", request.toDate, DbType.Date);
                parameters.Add("@EmployeeId", request.employeeId, DbType.Int32);
                parameters.Add("@BranchIds", $"{request.branchIds}", DbType.String);
                parameters.Add("@DepartmentIds", $"{request.departmentIds}", DbType.String);
                IEnumerable<OvertimeRequestModel>? lstResult = null;
                var dtResult = await connection.QueryMultipleAsync(StoreConstants.STORE_H1_OVERTIME_REQUEST_SELECT, param: parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                if (dtResult != null)
                {
                    lstResult = dtResult.Read<OvertimeRequestModel>();
                    if (request.documentId > 0)
                    {
                        var lstDetail = dtResult.Read<OvertimeRequest1Model>();
                        string jsonDetail = JsonConvert.SerializeObject(lstDetail);
                        lstResult = lstResult.Update(m => m.jsonDetail = jsonDetail);
                    }
                }
                return lstResult ?? new List<OvertimeRequestModel>();
            }
        }

        /// <summary>
        /// lấy danh sách cấu hình công
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<WorkConfigModel>> GetWorkConfig(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                int.TryParse(request.opt, out int year);
                if (year == 0) year = DateTime.Now.Year;
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId);
                parameters.Add("@BranchId", request.branchId);
                parameters.Add("@Year", year);
                parameters.Add("@Type", $"{request.type}");
                var lstResult = await connection.QueryAsync<WorkConfigModel>(StoreConstants.STORE_H1_WORK_CONFIG_SELECT, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return lstResult;
            }
        }

        /// <summary>
        /// lấy dữ liệu phân công ca làm việc
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ShiftAssignmentModel>> GetArrangeShift(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId);
                parameters.Add("@BranchId", request.branchId);
                parameters.Add("@DepartmentIds", request.departmentIds);
                parameters.Add("@Year", request.year);
                parameters.Add("@Month", request.month);
                IEnumerable<ShiftAssignmentModel>? lstResult = null;
                var dtResult = await connection.QueryMultipleAsync(StoreConstants.STORE_H1_ARRANGE_SHIFT_SELECT, param: parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                if (dtResult != null)
                {
                    lstResult = dtResult.Read<ShiftAssignmentModel>();
                    if(!lstResult.IsNullOrEmpty())
                    {
                        var lstDetail = dtResult.Read<ComboboxModel>();
                        string jsonDetail = JsonConvert.SerializeObject(lstDetail);
                        lstResult.First().jsonDetail = jsonDetail;
                    }    
                }
                return lstResult ?? new List<ShiftAssignmentModel>();
            }
        }

        /// <summary>
        /// lấy danh sách lịch làm việc của nhân viên
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TimesheetModel>> GetWorkShedule(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                int.TryParse(request.opt, out int year);
                int.TryParse(request.opt1, out int month);
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId);
                parameters.Add("@BranchId", request.branchId);
                parameters.Add("@EmployeeId", request.employeeId);
                parameters.Add("@Year", year);
                parameters.Add("@Month", month);
                parameters.Add("@Type", request.type);
                var lstResult = await connection.QueryAsync<TimesheetModel>(StoreConstants.STORE_H1_WORK_SCHEDULE_SELECT, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return lstResult;
            }
        }

        /// <summary>
        /// lấy thông tin quản lý phép năm
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<AnnualLeaveInfoModel>> GetAnnualLeaveInfo(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                int.TryParse(request.opt, out int year);
                if (year == 0) year = DateTime.Now.Year;
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId);
                parameters.Add("@BranchId", request.branchId);
                parameters.Add("@Year", year);
                parameters.Add("@DepartmentIds", request.departmentIds);
                parameters.Add("@EmployeeIds", request.opt3);
                parameters.Add("@StatusIds", request.opt2);
                var lstResult = await connection.QueryAsync<AnnualLeaveInfoModel>(StoreConstants.STORE_H1_ANNUAL_LEAVE_INFO_SELECT, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return lstResult;
            }
        }
        
        /// <summary>
        /// lấy dữ liệu chấm công của nhân viên
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<CheckInOutModel>> GetCheckInOut(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@MaChamCong", request.opt);
                parameters.Add("@NgayCham", request.fromDate);
                string query = "select T0.* from CheckInOuts as T0 with(nolock) where T0.MaChamCong = @MaChamCong and cast(T0.NgayCham as date) = @NgayCham";
                var lstResult = await connection.QueryAsync<CheckInOutModel>(query, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                return lstResult;
            }
        }

        /// <summary>
        /// lấy dữ liệu công làm việc của nhân viên trong tháng
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ShiftAssignmentModel>> GetWorkCalculate(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                int.TryParse(request.opt, out int year);
                int.TryParse(request.opt1, out int month);
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId);
                parameters.Add("@BranchId", request.branchId);
                parameters.Add("@Year", year);
                parameters.Add("@Month", month);
                parameters.Add("@DepartmentIds", $"{request.departmentIds}");
                parameters.Add("@StatusIds", $"{request.opt2}");
                parameters.Add("@EmployeeIds", $"{request.opt3}");
                parameters.Add("@Type", $"{request.type}");
                var lstResult = await connection.QueryAsync<ShiftAssignmentModel>(StoreConstants.STORE_H1_WORK_CALCULATION_SELECT, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return lstResult;
            }
        }

        /// <summary>
        /// Lấy ra thông tin các ngày bị thiếu giờ công
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ShiftAssignmentModel>> GetWorkingDayMissingHours(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                int.TryParse(request.opt, out int year);
                int.TryParse(request.opt1, out int month);
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId);
                parameters.Add("@BranchId", request.branchId);
                parameters.Add("@Year", year);
                parameters.Add("@Month", month);
                parameters.Add("@EmployeeId", request.employeeId);
                var lstResult = await connection.QueryAsync<ShiftAssignmentModel>(StoreConstants.STORE_H1_WORKING_DAY_MISSING_HOUR_SELECT, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return lstResult;
            }
        }

        /// <summary>
        /// lấy danh sách xác nhận giờ công
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ConfirmWorkingDayModel>> GetConfirmWorkingDay(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                request.fromDate ??= new DateTime(2000, 01, 01);
                request.toDate ??= DateTime.Now.AddMonths(1);
                var parameters = new DynamicParameters();
                parameters.Add("@ConfirmWorkingDayId", request.documentId, DbType.Int32);
                parameters.Add("@UserId", request.userId, DbType.Int32);
                parameters.Add("@BranchId", request.branchId, DbType.Int32);
                parameters.Add("@StatusIds", request.opt, DbType.String);
                parameters.Add("@FromDate", request.fromDate, DbType.Date);
                parameters.Add("@ToDate", request.toDate, DbType.Date);
                parameters.Add("@EmployeeId", request.employeeId, DbType.Int32);
                parameters.Add("@BranchIds", $"{request.branchIds}", DbType.String);
                parameters.Add("@DepartmentIds", $"{request.departmentIds}", DbType.String);
                IEnumerable<ConfirmWorkingDayModel>? lstResult = null;
                var dtResult = await connection.QueryMultipleAsync(StoreConstants.STORE_H1_CONFIRM_WORKING_HOUR_SELECT, param: parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                if (dtResult != null)
                {
                    lstResult = dtResult.Read<ConfirmWorkingDayModel>();
                    if (request.documentId > 0)
                    {
                        var lstDetail = dtResult.Read<ConfirmWorkingDay1Model>();
                        string jsonDetail = JsonConvert.SerializeObject(lstDetail);
                        lstResult = lstResult.Update(m => m.jsonDetail = jsonDetail);
                    }
                }
                return lstResult ?? new List<ConfirmWorkingDayModel>();
            }
        }
        
        /// <summary>
        /// lấy danh sách dữ liệu công đã lưu
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ShiftAssignmentModel>> GetAttendanceSummary(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                int.TryParse(request.opt, out int year);
                int.TryParse(request.opt1, out int month);
                if (year == 0) year = DateTime.Now.Year;
                if (month == 0) month = DateTime.Now.Month;
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId);
                parameters.Add("@BranchId", request.branchId);
                parameters.Add("@Year", year);
                parameters.Add("@Month", month);
                parameters.Add("@DepartmentIds", $"{request.departmentIds}");
                parameters.Add("@StatusIds", $"{request.opt2}");
                parameters.Add("@EmployeeIds", $"{request.opt3}");
                parameters.Add("@Type", $"{request.type}");
                var lstResult = await connection.QueryAsync<ShiftAssignmentModel>(StoreConstants.STORE_H1_ATTENDANCE_SUMMARY_SELECT, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
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
                    // kiểm tra trùng năm không & đang áp dụng
                    var checkExists = await _dbContext.LeaveConfigs.FirstOrDefaultAsync(m => m.Year == entity.Year && m.IsActive && m.IsDelete == false && m.BranchId == entity.BranchId);
                    if (checkExists != null)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = $"Thông số phép của năm [{checkExists.Year}] đã tồn tại";
                        return response;
                    }
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
                if(entity.IsActive)
                {
                    // kiểm tra trùng năm không & đang áp dụng
                    var checkExists = await _dbContext.LeaveConfigs.FirstOrDefaultAsync(m => m.Year == entity.Year && m.IsActive 
                            && m.IsDelete == false && m.Id != data.Id && m.BranchId == entity.BranchId);
                    if (checkExists != null)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = $"Thông số phép của năm [{checkExists.Year}] đã tồn tại";
                        return response;
                    }
                }
                data.BranchId = entity.BranchId;
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
                    // xuống store validate dữ liệu trước khi lưu
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", entity.UserSign, DbType.Int32);
                    parameters.Add("@BranchId", entity.BranchId, DbType.Int32);
                    parameters.Add("@EmployeeId", entity.EmployeeId, DbType.Int32);
                    parameters.Add("@ProcessKey", ProcessConstants.POST_LEAVE_REQUEST, DbType.String);
                    parameters.Add("@JHeader", JsonConvert.SerializeObject(entity), DbType.String);
                    parameters.Add("@JLine", JsonConvert.SerializeObject(lstEntity1), DbType.String);
                    var resultCheck = await connection.QueryFirstOrDefaultAsync<ResponseModel>(StoreConstants.STORE_H1_LEAVE_REQUEST_VALIDATE_CHECK, parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                    if(resultCheck == null || resultCheck.status != StatusCodes.Status200OK)
                    {
                        response.status = resultCheck?.status ?? StatusCodes.Status400BadRequest;
                        response.message = resultCheck?.message ?? MessageConstants.MESSAGE_IT_SUPPORT;
                        return response;
                    }  
                    // Đánh mã chứng từ
                    DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                    parameters = new DynamicParameters();
                    parameters.Add("@Type", GlobalConstants.TABLE_LEAVE_REQUEST, DbType.String);
                    parameters.Add("@BranchId", entity.BranchId, DbType.Int32);
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, @BranchId, '', '', '')";
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
                        entity1.IsDayOff = item.IsDayOff;
                        entity1.TotalLeaveDays = item.TotalLeaveDays;
                        entity1.BgColor = item.BgColor;
                        entity1.Symbol = item.Symbol;
                        entity1.HolidayId = item.HolidayId;
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
                
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    var data = await _dbContext.LeaveRequests.FirstOrDefaultAsync(m => m.Id == entity.Id);
                    if (data == null)
                    {
                        response.status = StatusCodes.Status404NotFound;
                        response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                        return response;
                    }
                    if (data.DateTracking != entity.DateTracking)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = MessageConstants.MESSAGE_DATA_CHECKING_MODIFIED;
                        return response;
                    }
                    // xuống store validate dữ liệu trước khi lưu
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@UserId", entity.UserSign, DbType.Int32);
                    parameters.Add("@BranchId", entity.BranchId, DbType.Int32);
                    parameters.Add("@EmployeeId", entity.EmployeeId, DbType.Int32);
                    parameters.Add("@ProcessKey", ProcessConstants.POST_LEAVE_REQUEST, DbType.String);
                    parameters.Add("@JHeader", JsonConvert.SerializeObject(entity), DbType.String);
                    parameters.Add("@JLine", JsonConvert.SerializeObject(lstEntity1), DbType.String);
                    var resultCheck = await connection.QueryFirstOrDefaultAsync<ResponseModel>(StoreConstants.STORE_H1_LEAVE_REQUEST_VALIDATE_CHECK, parameters
                    , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                    if (resultCheck == null || resultCheck.status != StatusCodes.Status200OK)
                    {
                        response.status = resultCheck?.status ?? StatusCodes.Status400BadRequest;
                        response.message = resultCheck?.message ?? MessageConstants.MESSAGE_IT_SUPPORT;
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
                    // bỏ dữ liệu củ đi
                    var lstLeaveRequest1s = await _dbContext.LeaveRequest1s.Where(m => m.LeaveRequestId == data.Id).ToListAsync();
                    if (!lstLeaveRequest1s.IsNullOrEmpty()) _dbContext.LeaveRequest1s.RemoveRange(lstLeaveRequest1s);
                    foreach (var item in lstEntity1)
                    {
                        LeaveRequest1s entity1 = new LeaveRequest1s();
                        entity1.LeaveRequestId = entity.Id;
                        entity1.DateOff = item.DateOff;
                        entity1.IsMorningBreak = item.IsMorningBreak;
                        entity1.IsAfternoonBreak = item.IsAfternoonBreak;
                        entity1.Remark = item.Remark;
                        entity1.IsDayOff = item.IsDayOff;
                        entity1.TotalLeaveDays = item.TotalLeaveDays;
                        entity1.BgColor = item.BgColor;
                        entity1.Symbol = item.Symbol;
                        entity1.HolidayId = item.HolidayId;
                        entity1.DateTracking = dateTimeNow;
                        entity1.UserSign = entity.UserSign;
                        await _dbContext.LeaveRequest1s.AddAsync(entity1);
                    }
                    await _dbContext.SaveChangesAsync();
                    await _dbContext.Database.CommitTransactionAsync();
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
        /// lưu thông tin xin phép nghỉ trong giờ
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateLeaveWorkingHours(string actionType, LeaveWorkingHours entity, IEnumerable<LeaveWorkingHour1s> lstEntity1)
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
                        // xuống store validate dữ liệu trước khi lưu
                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@UserId", entity.UserSign, DbType.Int32);
                        parameters.Add("@BranchId", entity.BranchId, DbType.Int32);
                        parameters.Add("@EmployeeId", entity.EmployeeId, DbType.Int32);
                        parameters.Add("@ProcessKey", ProcessConstants.POST_LEAVE_WORKING_HOUR, DbType.String);
                        parameters.Add("@JHeader", JsonConvert.SerializeObject(entity), DbType.String);
                        parameters.Add("@JLine", JsonConvert.SerializeObject(lstEntity1), DbType.String);
                        var resultCheck = await connection.QueryFirstOrDefaultAsync<ResponseModel>(StoreConstants.STORE_H1_LEAVE_REQUEST_VALIDATE_CHECK, parameters
                        , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                        if (resultCheck == null || resultCheck.status != StatusCodes.Status200OK)
                        {
                            response.status = resultCheck?.status ?? StatusCodes.Status400BadRequest;
                            response.message = resultCheck?.message ?? MessageConstants.MESSAGE_IT_SUPPORT;
                            return response;
                        }

                        // Đánh mã chứng từ
                        parameters = new DynamicParameters();
                        parameters.Add("@Type", GlobalConstants.TABLE_LEAVE_WORKING_HOURS, DbType.String);
                        parameters.Add("@BranchId", entity.BranchId, DbType.Int32);
                        string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, @BranchId, '', '', '')";
                        string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                        if (string.IsNullOrEmpty(voucherNo))
                        {
                            response.status = StatusCodes.Status204NoContent;
                            response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                            return response;
                        }
                        await _dbContext.Database.BeginTransactionAsync();
                        isTrans = true;
                        entity.Id = await _dbContext.LeaveWorkingHours.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                        entity.VoucherNo = voucherNo;
                        entity.DateTracking = dateTimeNow;
                        entity.CreateDate = dateTimeNow;
                        await _dbContext.LeaveWorkingHours.AddAsync(entity);
                        // thêm chi tiết
                        foreach (var item in lstEntity1)
                        {
                            LeaveWorkingHour1s entity1 = new LeaveWorkingHour1s();
                            entity1.LeaveWorkingHourId = entity.Id;
                            entity1.DateOff = item.DateOff;
                            entity1.FromTime = item.FromTime;
                            entity1.ToTime = item.ToTime;
                            entity1.TotalHours = item.TotalHours;
                            entity1.Remark = item.Remark;
                            entity1.IsDayOff = item.IsDayOff;
                            entity1.BgColor = item.BgColor;
                            entity1.Symbol = item.Symbol;
                            entity1.HolidayId = item.HolidayId;
                            entity1.ShiftCode = item.ShiftCode;
                            entity1.StartDate = item.StartDate;
                            entity1.EndDate = item.EndDate;
                            entity1.StartBreakTime = item.StartBreakTime;
                            entity1.EndBreakTime = item.EndBreakTime;
                            entity1.DateTracking = dateTimeNow;
                            entity1.UserSign = entity.UserSign;
                            await _dbContext.LeaveWorkingHour1s.AddAsync(entity1);
                        }
                        await _dbContext.SaveChangesAsync();
                        await _dbContext.Database.CommitTransactionAsync();
                        response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                        response.data = entity.Id;
                    }
                }
                else
                {
                    using (var connection = _dapperDbContext.CreateConnection())
                    {
                        var data = await _dbContext.LeaveWorkingHours.FirstOrDefaultAsync(m => m.Id == entity.Id);
                        if (data == null)
                        {
                            response.status = StatusCodes.Status404NotFound;
                            response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                            return response;
                        }
                        if (data.DateTracking != entity.DateTracking)
                        {
                            response.status = StatusCodes.Status409Conflict;
                            response.message = MessageConstants.MESSAGE_DATA_CHECKING_MODIFIED;
                            return response;
                        }
                        // xuống store validate dữ liệu trước khi lưu
                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@UserId", entity.UserSign, DbType.Int32);
                        parameters.Add("@BranchId", entity.BranchId, DbType.Int32);
                        parameters.Add("@EmployeeId", entity.EmployeeId, DbType.Int32);
                        parameters.Add("@ProcessKey", ProcessConstants.POST_LEAVE_WORKING_HOUR, DbType.String);
                        parameters.Add("@JHeader", JsonConvert.SerializeObject(entity), DbType.String);
                        parameters.Add("@JLine", JsonConvert.SerializeObject(lstEntity1), DbType.String);
                        var resultCheck = await connection.QueryFirstOrDefaultAsync<ResponseModel>(StoreConstants.STORE_H1_LEAVE_REQUEST_VALIDATE_CHECK, parameters
                        , commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                        if (resultCheck == null || resultCheck.status != StatusCodes.Status200OK)
                        {
                            response.status = resultCheck?.status ?? StatusCodes.Status400BadRequest;
                            response.message = resultCheck?.message ?? MessageConstants.MESSAGE_IT_SUPPORT;
                            return response;
                        }
                        data.EmployeeId = entity.EmployeeId;
                        data.EmployeeSignatureId = entity.EmployeeSignatureId;
                        data.DepartmentId = entity.DepartmentId;
                        data.StatusCode = entity.StatusCode;
                        data.FromDate = entity.FromDate;
                        data.ToDate = entity.ToDate;
                        data.Remark = entity.Remark;
                        data.RequestType = entity.RequestType;
                        data.TotalHours = entity.TotalHours;
                        data.DateTracking = dateTimeNow;
                        data.UpdateDate = dateTimeNow;
                        data.UserSign2 = entity.UserSign2;
                        await _dbContext.Database.BeginTransactionAsync();
                        isTrans = true;
                        _dbContext.LeaveWorkingHours.Attach(data);
                        _dbContext.Entry(data).State = EntityState.Modified;
                        // thêm chi tiết đề nghị nghỉ phép
                        // bỏ dữ liệu củ đi
                        var lstLeaveRequest1s = await _dbContext.LeaveWorkingHour1s.Where(m => m.LeaveWorkingHourId == data.Id).ToListAsync();
                        if (!lstLeaveRequest1s.IsNullOrEmpty()) _dbContext.LeaveWorkingHour1s.RemoveRange(lstLeaveRequest1s);
                        foreach (var item in lstEntity1)
                        {
                            LeaveWorkingHour1s entity1 = new LeaveWorkingHour1s();
                            entity1.LeaveWorkingHourId = entity.Id;
                            entity1.DateOff = item.DateOff;
                            entity1.FromTime = item.FromTime;
                            entity1.ToTime = item.ToTime;
                            entity1.TotalHours = item.TotalHours;
                            entity1.Remark = item.Remark;
                            entity1.IsDayOff = item.IsDayOff;
                            entity1.BgColor = item.BgColor;
                            entity1.Symbol = item.Symbol;
                            entity1.HolidayId = item.HolidayId;
                            entity1.ShiftCode = item.ShiftCode;
                            entity1.StartDate = item.StartDate;
                            entity1.EndDate = item.EndDate;
                            entity1.StartBreakTime = item.StartBreakTime;
                            entity1.EndBreakTime = item.EndBreakTime;
                            entity1.DateTracking = dateTimeNow;
                            entity1.UserSign = entity.UserSign;
                            await _dbContext.LeaveWorkingHour1s.AddAsync(entity1);
                        }
                        await _dbContext.SaveChangesAsync();
                        await _dbContext.Database.CommitTransactionAsync();
                        response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                        response.data = data.Id;
                    }    
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
                    var checkExists = await _dbContext.HolidayCatagories.AnyAsync(m => !(entity.ToDate.Date < m.FromDate.Date || entity.FromDate.Date > m.ToDate.Date) && m.BranchId == entity.BranchId);
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
                data.BranchId = entity.BranchId;
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

        /// <summary>
        /// Thêm mới chứng từ đăng kí đổi ca làm việc
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lstEntity1"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddShiftChangeRequest(ShiftChanges entity, IEnumerable<ShiftChange1s> lstEntity1)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    // kiểm tra dữ liệu công ông đây chốt chưa -> nếu chốt công không cho tạo
                    var checkLocked = await _dbContext.AttendanceSummarys.FirstOrDefaultAsync(m => m.EmployeeId == entity.EmployeeId && m.IsDelete == false
                                            && m.IsLocked == true && m.Month == entity.FromDate!.Value.Month
                                            && m.Year == entity.FromDate!.Value.Year);
                    if (checkLocked != null)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = $"Nhân viên {checkLocked.EmployeeCode} đã được khóa kỳ dữ liệu công tháng {checkLocked.Month} năm {checkLocked.Year}";
                        return response;
                    }
                    var checkExists = await _dbContext.ShiftChanges.FirstOrDefaultAsync(m => !(entity.ToDate!.Value.Date < m.FromDate!.Value.Date || entity.FromDate!.Value.Date > m.ToDate!.Value.Date)
                                            && m.EmployeeId == entity.EmployeeId
                                            && m.StatusCode != CommonConstants.STATUS_CODE_CANCELED && m.StatusCode != CommonConstants.STATUS_CODE_DENY); // bỏ 2 tình trạng hủy
                    if (checkExists != null)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = $"Từ ngày hoặc đến ngày đổi ca đã tồn tại trong hệ thống. Số chứng từ [{checkExists.VoucherNo}]";
                        return response;
                    }
                    DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@Type", GlobalConstants.TABLE_SHIFT_CHANGE_REQUEST, DbType.String);
                    parameters.Add("@BranchId", entity.BranchId, DbType.Int32);
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, @BranchId, '', '', '')";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    await _dbContext.Database.BeginTransactionAsync();
                    isTrans = true;
                    entity.Id = await _dbContext.ShiftChanges.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.VoucherNo = voucherNo;
                    entity.DateTracking = dateTimeNow;
                    entity.CreateDate = dateTimeNow;
                    await _dbContext.ShiftChanges.AddAsync(entity);
                    // thêm chi tiết đề nghị nghỉ phép
                    foreach (var item in lstEntity1)
                    {
                        ShiftChange1s entity1 = new ShiftChange1s();
                        entity1.ShiftChangeId = entity.Id;
                        entity1.DateChange = item.DateChange;
                        entity1.ShiftCode1 = item.ShiftCode1;
                        entity1.ShiftCode2 = item.ShiftCode2;
                        entity1.Remark = item.Remark;
                        entity1.IsDayOff = item.IsDayOff;
                        entity1.BgColor = item.BgColor;
                        entity1.Symbol = item.Symbol;
                        entity1.HolidayId = item.HolidayId;
                        entity1.DateTracking = dateTimeNow;
                        entity1.UserSign = entity.UserSign;
                        await _dbContext.ShiftChange1s.AddAsync(entity1);
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
        /// cập nhật thông tin đăng kí đổi ca làm việc
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lstEntity1"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateShiftChangeRequest(ShiftChanges entity, IEnumerable<ShiftChange1s> lstEntity1)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.ShiftChanges.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                if (data.DateTracking != entity.DateTracking)
                {
                    response.status = StatusCodes.Status409Conflict;
                    response.message = MessageConstants.MESSAGE_DATA_CHECKING_MODIFIED;
                    return response;
                }
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                data.EmployeeId = entity.EmployeeId;
                data.EmployeeSignatureId = entity.EmployeeSignatureId;
                data.DepartmentId = entity.DepartmentId;
                data.StatusCode = entity.StatusCode;
                data.FromDate = entity.FromDate;
                data.ToDate = entity.ToDate;
                data.Reason = entity.Reason;
                data.DateTracking = dateTimeNow;
                data.UpdateDate = dateTimeNow;
                data.UserSign2 = entity.UserSign2;
                await _dbContext.Database.BeginTransactionAsync();
                isTrans = true;
                _dbContext.ShiftChanges.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                // thêm chi tiết đổi ca
                // bỏ dữ liệu củ đi
                var lstShiftChange1s = await _dbContext.ShiftChange1s.Where(m => m.ShiftChangeId == data.Id).ToListAsync();
                if (!lstShiftChange1s.IsNullOrEmpty()) _dbContext.ShiftChange1s.RemoveRange(lstShiftChange1s);
                foreach (var item in lstEntity1)
                {
                    ShiftChange1s entity1 = new ShiftChange1s();
                    entity1.ShiftChangeId = entity.Id;
                    entity1.DateChange = item.DateChange;
                    entity1.ShiftCode1 = item.ShiftCode1;
                    entity1.ShiftCode2 = item.ShiftCode2;
                    entity1.Remark = item.Remark;
                    entity1.IsDayOff = item.IsDayOff;
                    entity1.BgColor = item.BgColor;
                    entity1.Symbol = item.Symbol;
                    entity1.HolidayId = item.HolidayId;
                    entity1.DateTracking = dateTimeNow;
                    entity1.UserSign = entity.UserSign;
                    await _dbContext.ShiftChange1s.AddAsync(entity1);
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
        /// Thêm mới chứng từ đề nghị làm thêm
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lstEntity1"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddOvertimeRequest(OvertimeRequests entity, IEnumerable<OvertimeRequest1s> lstEntity1)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    // kiểm tra dữ liệu công ông đây chốt chưa -> nếu chốt công không cho tạo
                    var checkLocked = await _dbContext.AttendanceSummarys.FirstOrDefaultAsync(m => m.EmployeeId == entity.EmployeeId && m.IsDelete == false
                                            && m.IsLocked == true && m.Month == entity.FromDate!.Value.Month
                                            && m.Year == entity.FromDate!.Value.Year);
                    if (checkLocked != null)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = $"Nhân viên {checkLocked.EmployeeCode} đã được khóa kỳ dữ liệu công tháng {checkLocked.Month} năm {checkLocked.Year}";
                        return response;
                    }

                    // kiểm tra số giờ tăng ca có vượt ngưỡng không
                    EnumCatagories? enumConfig = await _dbContext.EnumCatagories.FirstOrDefaultAsync(m => m.EnumType == CommonConstants.MAX_OVERTIME_REQUEST);
                    double.TryParse(enumConfig?.Value, out double totalHours);
                    if (entity.TotalHours > totalHours)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = $"Tổng số giờ tăng ca không được vượt quá {totalHours.FormatCurrency()} giờ";
                        return response;
                    }    
                    // kiểm tra trùng giờ tăng ca
                    var checkExists = await _dbContext.OvertimeRequests.FirstOrDefaultAsync(m => !(entity.ToDate!.Value.Date < m.FromDate!.Value.Date || entity.FromDate!.Value.Date > m.ToDate!.Value.Date) 
                                            && m.EmployeeId == entity.EmployeeId
                                            && m.StatusCode != CommonConstants.STATUS_CODE_CANCELED && m.StatusCode != CommonConstants.STATUS_CODE_DENY);
                    if (checkExists != null)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = $"Từ ngày hoặc đến ngày đã tồn tại trong hệ thống. Số chứng từ [{checkExists.VoucherNo}]";
                        return response;
                    }
                    DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@Type", GlobalConstants.TABLE_OVERTIME_REQUEST, DbType.String);
                    parameters.Add("@BranchId", entity.BranchId, DbType.Int32);
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, @BranchId, '', '', '')";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    await _dbContext.Database.BeginTransactionAsync();
                    isTrans = true;
                    entity.Id = await _dbContext.OvertimeRequests.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.VoucherNo = voucherNo;
                    entity.DateTracking = dateTimeNow;
                    entity.CreateDate = dateTimeNow;
                    await _dbContext.OvertimeRequests.AddAsync(entity);
                    // thêm chi tiết đề nghị nghỉ phép
                    foreach (var item in lstEntity1)
                    {
                        OvertimeRequest1s entity1 = new OvertimeRequest1s();
                        entity1.OvertimeRequestId = entity.Id;
                        entity1.ShiftCode = item.ShiftCode;
                        entity1.OvertimeDate = item.OvertimeDate;
                        entity1.StartTime = item.StartTime;
                        entity1.EndTime = item.EndTime;
                        entity1.StartBreakTime = item.StartBreakTime;
                        entity1.EndBreakTime = item.EndBreakTime;
                        entity1.Remark = item.Remark;
                        entity1.TotalWorkingHours = item.TotalWorkingHours;
                        entity1.IsDayOff = item.IsDayOff;
                        entity1.BgColor = item.BgColor;
                        entity1.Symbol = item.Symbol;
                        entity1.HolidayId = item.HolidayId;
                        entity1.DateTracking = dateTimeNow;
                        entity1.UserSign = entity.UserSign;
                        await _dbContext.OvertimeRequest1s.AddAsync(entity1);
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
        /// cập nhật thông tin đề nghị làm thêm
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lstEntity1"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateOvertimeRequest(OvertimeRequests entity, IEnumerable<OvertimeRequest1s> lstEntity1)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.OvertimeRequests.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                if (data.DateTracking != entity.DateTracking)
                {
                    response.status = StatusCodes.Status409Conflict;
                    response.message = MessageConstants.MESSAGE_DATA_CHECKING_MODIFIED;
                    return response;
                }
                // kiểm tra số giờ tăng ca có vượt ngưỡng không
                EnumCatagories? enumConfig = await _dbContext.EnumCatagories.FirstOrDefaultAsync(m => m.EnumType == CommonConstants.MAX_OVERTIME_REQUEST);
                double.TryParse(enumConfig?.Value, out double totalHours);
                if (entity.TotalHours > totalHours)
                {
                    response.status = StatusCodes.Status409Conflict;
                    response.message = $"Tổng số giờ tăng ca không được vượt quá {totalHours.FormatCurrency()} giờ";
                    return response;
                }
                // kiểm tra trùng giờ tăng ca
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                data.EmployeeId = entity.EmployeeId;
                data.EmployeeSignatureId = entity.EmployeeSignatureId;
                data.DepartmentId = entity.DepartmentId;
                data.RequestType = entity.RequestType;
                data.StatusCode = entity.StatusCode;
                data.FromDate = entity.FromDate;
                data.ToDate = entity.ToDate;
                data.Reason = entity.Reason;
                data.TotalHours = entity.TotalHours;
                data.DateTracking = dateTimeNow;
                data.UpdateDate = dateTimeNow;
                data.UserSign2 = entity.UserSign2;
                await _dbContext.Database.BeginTransactionAsync();
                isTrans = true;
                _dbContext.OvertimeRequests.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                // thêm chi tiết đề nghị tăng ca
                // bỏ dữ liệu củ đi
                var lstOvertimeRequest1s = await _dbContext.OvertimeRequest1s.Where(m => m.OvertimeRequestId == data.Id).ToListAsync();
                if (!lstOvertimeRequest1s.IsNullOrEmpty()) _dbContext.OvertimeRequest1s.RemoveRange(lstOvertimeRequest1s);
                foreach (var item in lstEntity1)
                {
                    OvertimeRequest1s entity1 = new OvertimeRequest1s();
                    entity1.OvertimeRequestId = entity.Id;
                    entity1.ShiftCode = item.ShiftCode;
                    entity1.OvertimeDate = item.OvertimeDate;
                    entity1.StartTime = item.StartTime;
                    entity1.EndTime = item.EndTime;
                    entity1.StartBreakTime = item.StartBreakTime;
                    entity1.EndBreakTime = item.EndBreakTime;
                    entity1.Remark = item.Remark;
                    entity1.TotalWorkingHours = item.TotalWorkingHours;
                    entity1.IsDayOff = item.IsDayOff;
                    entity1.BgColor = item.BgColor;
                    entity1.Symbol = item.Symbol;
                    entity1.HolidayId = item.HolidayId;
                    entity1.DateTracking = dateTimeNow;
                    entity1.UserSign = entity.UserSign;
                    await _dbContext.OvertimeRequest1s.AddAsync(entity1);
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
        /// phát sinh dữ liệu cấu hình thông số công chi tiết
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseModel> GenerateWorkConfig(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                ResponseModel response = new ResponseModel();
                int.TryParse(request.opt, out int year);
                if (year == 0) year = DateTime.Now.Year;
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId);
                parameters.Add("@BranchId", request.branchId);
                parameters.Add("@Year", year);
                parameters.Add("@Type", request.type);
                var lstResult = await connection.QueryAsync<WorkConfigModel>(StoreConstants.STORE_H1_GENERATE_WORK_CONFIG_UPDATE, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                if (!lstResult.IsNullOrEmpty())
                {
                    response.status = lstResult!.First().status;
                    response.message = lstResult!.First().message ?? MessageConstants.MESSAGE_IT_SUPPORT;
                    response.returnValue = JsonConvert.SerializeObject(lstResult!);
                }
                return response;
            }
            
        }

        /// <summary>
        /// /Phát sinh dữ liệu công của nhân viên
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseModel> GenerateTimesheets(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                int.TryParse(request.opt, out int year);
                if (year == 0) year = DateTime.Now.Year;
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", request.userId);
                parameters.Add("@BranchId", request.branchId);
                parameters.Add("@StartDate", request.fromDate);
                parameters.Add("@EmployeeId", request.employeeId);
                parameters.Add("@DepartmentIds", request.departmentIds); // mã phòng ban
                var lstResult = await connection.QueryFirstAsync<ResponseModel>(StoreConstants.STORE_H1_GENERATE_TIMESHEET_DATA_UPDATE, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.StoredProcedure);
                return lstResult;
            }

        }

        /// <summary>
        /// lưu thông tin phân ca làm việc
        /// </summary>
        /// <param name="lstEntity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateShiftAssignment(IEnumerable<ShiftAssignments> lstEntity)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                await _dbContext.Database.BeginTransactionAsync();
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                foreach (var entity in lstEntity)
                {
                    var data = await _dbContext.ShiftAssignments.FirstOrDefaultAsync(m => m.EmployeeId == entity.EmployeeId
                        && m.Year == m.Year && m.Month == m.Month && m.BranchId == m.BranchId);
                    if(data == null)
                    {
                        // thêm mới
                        entity.Id = 0;
                        entity.DateTracking = dateTimeNow;
                        entity.CreateDate = dateTimeNow;
                        await _dbContext.ShiftAssignments.AddAsync(entity);
                        continue;
                    }
                    // cập nhật dữ liệu
                    data.DepartmentId = entity.DepartmentId;
                    data.TitleId = entity.TitleId;
                    data.ShiftCode = entity.ShiftCode;
                    data.N01 = entity.N01;
                    data.N02 = entity.N02;
                    data.N03 = entity.N03;
                    data.N04 = entity.N04;
                    data.N05 = entity.N05;
                    data.N06 = entity.N06;
                    data.N07 = entity.N07;
                    data.N08 = entity.N08;
                    data.N09 = entity.N09;
                    data.N10 = entity.N10;
                    data.N11 = entity.N11;
                    data.N12 = entity.N12;
                    data.N13 = entity.N13;
                    data.N14 = entity.N14;
                    data.N15 = entity.N15;
                    data.N16 = entity.N16;
                    data.N17 = entity.N17;
                    data.N18 = entity.N18;
                    data.N19 = entity.N19;
                    data.N20 = entity.N20;
                    data.N21 = entity.N21;
                    data.N22 = entity.N22;
                    data.N23 = entity.N23;
                    data.N24 = entity.N24;
                    data.N25 = entity.N25;
                    data.N26 = entity.N26;
                    data.N27 = entity.N27;
                    data.N28 = entity.N28;
                    data.N29 = entity.N29;
                    data.N30 = entity.N30;
                    data.N31 = entity.N31;
                    data.DateTracking = dateTimeNow;
                    data.UpdateDate = dateTimeNow;
                    data.UserSign2 = entity.UserSign2;
                    _dbContext.ShiftAssignments.Attach(data);
                    _dbContext.Entry(data).State = EntityState.Modified;
                }
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }
        }
        
        /// <summary>
        /// cập nhật lại thông tin cấu hình mặc định của ngày công
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateWorkConfig(WorkConfigs entity)
        {
            ResponseModel response = new ResponseModel();
            bool isTrans = false;
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                // Tạo mới
                await _dbContext.Database.BeginTransactionAsync();
                isTrans = true;
                // lấy ra thông tin cấu hình mặc định của hệ thống
                var lstWorkConfigDefault = await _dbContext.WorkConfigs.Where(m => m.IsDelete == false && m.WorkConfigType == "DEFAULT").ToListAsync();
                if(!lstWorkConfigDefault.IsNullOrEmpty())
                {
                    // nếu có của chi nhánh đấy thì lấy theo chi nhánh
                    var lstWorkConfigByBranch = lstWorkConfigDefault.Where(m => m.BranchId == entity.BranchId).ToList();
                    foreach (var itemUpdate in lstWorkConfigByBranch)
                    {
                        itemUpdate.IsDelete = true;
                        itemUpdate.DeleteReason = $"User: {entity.UserSign2} đã cập nhật thông tin cấu hình mới";
                        itemUpdate.DateTracking = dateTimeNow;
                        itemUpdate.UserSign2 = entity.UserSign2;
                        _dbContext.WorkConfigs.Attach(itemUpdate);
                        _dbContext.Entry(itemUpdate).State = EntityState.Modified;
                    }    
                }
                entity.Id = 0;
                entity.IsDelete = false;
                entity.WorkConfigType = "DEFAULT";
                entity.DateTracking = dateTimeNow;
                entity.CreateDate = dateTimeNow;
                await _dbContext.WorkConfigs.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }
        }
        
        /// <summary>
        /// cập nhật thông tin chốt công
        /// </summary>
        /// <param name="isLocked"></param>
        /// <param name="userId"></param>
        /// <param name="lstEntity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateAttendanceSummary(bool isLocked, int userId, IEnumerable<AttendanceSummarys> lstEntity)
        {
            ResponseModel response = new ResponseModel();
            bool isTrans = false;
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                // Tạo mới
                await _dbContext.Database.BeginTransactionAsync();
                isTrans = true;
                string pattern = @"<span[^>]*>(.*?)<\/span>";
                foreach (var entity in lstEntity)
                {
                    var data = await _dbContext.AttendanceSummarys.FirstOrDefaultAsync(m => m.EmployeeId == entity.EmployeeId && m.Month == entity.Month && m.Year == entity.Year);
                    if (data == null)
                    {
                        entity.CreateDate = dateTimeNow;
                        entity.DateTracking = dateTimeNow;
                        entity.ShiftCode = entity.ShiftCode ?? "-";
                        entity.UserSign = userId;
                        entity.IsLocked = isLocked;
                        entity.N01 = entity.N01;
                        entity.N02 = entity.N02;
                        entity.N03 = entity.N03;
                        entity.N04 = entity.N04;
                        entity.N05 = entity.N05;
                        entity.N06 = entity.N06;
                        entity.N07 = entity.N07;
                        entity.N08 = entity.N08;
                        entity.N09 = entity.N09;
                        entity.N10 = entity.N10;
                        entity.N11 = entity.N11;
                        entity.N12 = entity.N12;
                        entity.N13 = entity.N13;
                        entity.N14 = entity.N14;
                        entity.N15 = entity.N15;
                        entity.N16 = entity.N16;
                        entity.N17 = entity.N17;
                        entity.N18 = entity.N18;
                        entity.N19 = entity.N19;
                        entity.N20 = entity.N20;
                        entity.N21 = entity.N21;
                        entity.N22 = entity.N22;
                        entity.N23 = entity.N23;
                        entity.N24 = entity.N24;
                        entity.N25 = entity.N25;
                        entity.N26 = entity.N26;
                        entity.N27 = entity.N27;
                        entity.N28 = entity.N28;
                        entity.N29 = entity.N29;
                        entity.N30 = entity.N30;
                        entity.N31 = entity.N31;


                        //entity.N01 = Regex.Replace(entity.N01 + "", pattern, "$1");
                        //entity.N02 = Regex.Replace(entity.N02 + "", pattern, "$1");
                        //entity.N03 = Regex.Replace(entity.N03 + "", pattern, "$1");
                        //entity.N04 = Regex.Replace(entity.N04 + "", pattern, "$1");
                        //entity.N05 = Regex.Replace(entity.N05 + "", pattern, "$1");
                        //entity.N06 = Regex.Replace(entity.N06 + "", pattern, "$1");
                        //entity.N07 = Regex.Replace(entity.N07 + "", pattern, "$1");
                        //entity.N08 = Regex.Replace(entity.N08 + "", pattern, "$1");
                        //entity.N09 = Regex.Replace(entity.N09 + "", pattern, "$1");
                        //entity.N10 = Regex.Replace(entity.N10 + "", pattern, "$1");
                        //entity.N11 = Regex.Replace(entity.N11 + "", pattern, "$1");
                        //entity.N12 = Regex.Replace(entity.N12 + "", pattern, "$1");
                        //entity.N13 = Regex.Replace(entity.N13 + "", pattern, "$1");
                        //entity.N14 = Regex.Replace(entity.N14 + "", pattern, "$1");
                        //entity.N15 = Regex.Replace(entity.N15 + "", pattern, "$1");
                        //entity.N16 = Regex.Replace(entity.N16 + "", pattern, "$1");
                        //entity.N17 = Regex.Replace(entity.N17 + "", pattern, "$1");
                        //entity.N18 = Regex.Replace(entity.N18 + "", pattern, "$1");
                        //entity.N19 = Regex.Replace(entity.N19 + "", pattern, "$1");
                        //entity.N20 = Regex.Replace(entity.N20 + "", pattern, "$1");
                        //entity.N21 = Regex.Replace(entity.N21 + "", pattern, "$1");
                        //entity.N22 = Regex.Replace(entity.N22 + "", pattern, "$1");
                        //entity.N23 = Regex.Replace(entity.N23 + "", pattern, "$1");
                        //entity.N24 = Regex.Replace(entity.N24 + "", pattern, "$1");
                        //entity.N25 = Regex.Replace(entity.N25 + "", pattern, "$1");
                        //entity.N26 = Regex.Replace(entity.N26 + "", pattern, "$1");
                        //entity.N27 = Regex.Replace(entity.N27 + "", pattern, "$1");
                        //entity.N28 = Regex.Replace(entity.N28 + "", pattern, "$1");
                        //entity.N29 = Regex.Replace(entity.N29 + "", pattern, "$1");
                        //entity.N30 = Regex.Replace(entity.N30 + "", pattern, "$1");
                        //entity.N31 = Regex.Replace(entity.N31 + "", pattern, "$1");
                        await _dbContext.AttendanceSummarys.AddAsync(entity);
                        continue;
                    }
                    if (data.IsLocked)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = $"Nhân viên {data.EmployeeCode} đã được khóa kỳ dữ liệu công!!!";
                        if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                        return response;
                    }
                    // cập nhật dữ liệu
                    data.EmployeeName = entity.EmployeeName;
                    data.BranchId = entity.BranchId;
                    data.DepartmentId = entity.DepartmentId;
                    data.DepartmentCode = entity.DepartmentCode;
                    data.DepartmentName = entity.DepartmentName;
                    data.PositionId = entity.PositionId;
                    data.PositionCode = entity.PositionCode;
                    data.PositionName = entity.PositionName;
                    data.TitleId = entity.TitleId;
                    data.TitleCode = entity.TitleCode;
                    data.TitleName = entity.TitleName;
                    data.ShiftCode = entity.ShiftCode ?? "-";
                    data.TNC = entity.TNC;
                    data.CDM = entity.CDM;
                    data.CTT = entity.CTT;
                    data.NL = entity.NL;
                    data.NPN = entity.NPN;
                    data.NCD = entity.NCD;
                    data.NPKL = entity.NPKL;
                    data.NB = entity.NB;
                    data.NKP = entity.NKP;
                    data.CTPC = entity.CTPC;
                    data.TGDLTVS = entity.TGDLTVS;
                    data.SLDLTVS = entity.SLDLTVS;
                    data.SGT = entity.SGT;
                    data.SGTC = entity.SGTC;
                    data.GCTC = entity.GCTC;
                    data.TGTC = entity.TGTC;
                    data.SGTCTC = entity.SGTCTC;
                    data.SGTCTT = entity.SGTCTT;
                    data.SGTCKT = entity.SGTCKT;
                    data.IsLocked = entity.IsLocked;

                    data.N01 = entity.N01;
                    data.N02 = entity.N02;
                    data.N03 = entity.N03;
                    data.N04 = entity.N04;
                    data.N05 = entity.N05;
                    data.N06 = entity.N06;
                    data.N07 = entity.N07;
                    data.N08 = entity.N08;
                    data.N09 = entity.N09;
                    data.N10 = entity.N10;
                    data.N11 = entity.N11;
                    data.N12 = entity.N12;
                    data.N13 = entity.N13;
                    data.N14 = entity.N14;
                    data.N15 = entity.N15;
                    data.N16 = entity.N16;
                    data.N17 = entity.N17;
                    data.N18 = entity.N18;
                    data.N19 = entity.N19;
                    data.N20 = entity.N20;
                    data.N21 = entity.N21;
                    data.N22 = entity.N22;
                    data.N23 = entity.N23;
                    data.N24 = entity.N24;
                    data.N25 = entity.N25;
                    data.N26 = entity.N26;
                    data.N27 = entity.N27;
                    data.N28 = entity.N28;
                    data.N29 = entity.N29;
                    data.N30 = entity.N30;
                    data.N31 = entity.N31;
                    //data.N01 = Regex.Replace(entity.N01 + "", pattern, "$1");
                    //data.N02 = Regex.Replace(entity.N02 + "", pattern, "$1");
                    //data.N03 = Regex.Replace(entity.N03 + "", pattern, "$1");
                    //data.N04 = Regex.Replace(entity.N04 + "", pattern, "$1");
                    //data.N05 = Regex.Replace(entity.N05 + "", pattern, "$1");
                    //data.N06 = Regex.Replace(entity.N06 + "", pattern, "$1");
                    //data.N07 = Regex.Replace(entity.N07 + "", pattern, "$1");
                    //data.N08 = Regex.Replace(entity.N08 + "", pattern, "$1");
                    //data.N09 = Regex.Replace(entity.N09 + "", pattern, "$1");
                    //data.N10 = Regex.Replace(entity.N10 + "", pattern, "$1");
                    //data.N11 = Regex.Replace(entity.N11 + "", pattern, "$1");
                    //data.N12 = Regex.Replace(entity.N12 + "", pattern, "$1");
                    //data.N13 = Regex.Replace(entity.N13 + "", pattern, "$1");
                    //data.N14 = Regex.Replace(entity.N14 + "", pattern, "$1");
                    //data.N15 = Regex.Replace(entity.N15 + "", pattern, "$1");
                    //data.N16 = Regex.Replace(entity.N16 + "", pattern, "$1");
                    //data.N17 = Regex.Replace(entity.N17 + "", pattern, "$1");
                    //data.N18 = Regex.Replace(entity.N18 + "", pattern, "$1");
                    //data.N19 = Regex.Replace(entity.N19 + "", pattern, "$1");
                    //data.N20 = Regex.Replace(entity.N20 + "", pattern, "$1");
                    //data.N21 = Regex.Replace(entity.N21 + "", pattern, "$1");
                    //data.N22 = Regex.Replace(entity.N22 + "", pattern, "$1");
                    //data.N23 = Regex.Replace(entity.N23 + "", pattern, "$1");
                    //data.N24 = Regex.Replace(entity.N24 + "", pattern, "$1");
                    //data.N25 = Regex.Replace(entity.N25 + "", pattern, "$1");
                    //data.N26 = Regex.Replace(entity.N26 + "", pattern, "$1");
                    //data.N27 = Regex.Replace(entity.N27 + "", pattern, "$1");
                    //data.N28 = Regex.Replace(entity.N28 + "", pattern, "$1");
                    //data.N29 = Regex.Replace(entity.N29 + "", pattern, "$1");
                    //data.N30 = Regex.Replace(entity.N30 + "", pattern, "$1");
                    //data.N31 = Regex.Replace(entity.N31 + "", pattern, "$1");
                    data.IsLocked = isLocked;
                    data.DateTracking = dateTimeNow;
                    data.UpdateDate = dateTimeNow;
                    data.UserSign2 = userId;
                    _dbContext.AttendanceSummarys.Attach(data);
                    _dbContext.Entry(data).State = EntityState.Modified;

                }   
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message =  MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Mở khóa kỳ công
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="lstEntity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UnLockAttendanceSummary(int userId, IEnumerable<AttendanceSummarys> lstEntity)
        {
            ResponseModel response = new ResponseModel();
            bool isTrans = false;
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                // Tạo mới
                await _dbContext.Database.BeginTransactionAsync();
                isTrans = true;
                foreach (var entity in lstEntity)
                {
                    var data = await _dbContext.AttendanceSummarys.FirstOrDefaultAsync(m => m.EmployeeId == entity.EmployeeId && m.Month == entity.Month && m.Year == entity.Year);
                    if (data == null || !data.IsLocked)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = $"Nhân viên {entity.EmployeeCode} chưa được khóa kỳ dữ liệu công!!!";
                        if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                        return response;
                    }
                    var data2 = await _dbContext.Payrolls.FirstOrDefaultAsync(m => m.EmployeeId == entity.EmployeeId && m.Month == entity.Month && m.Year == entity.Year && m.IsLocked);
                    if(data2 != null)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = $"Nhân viên {data.EmployeeCode} đã được khóa kỳ dữ liệu lương. Không thể mở khóa kỳ công!!!";
                        if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                        return response;
                    }    
                    data.IsLocked = false;
                    data.DateTracking = dateTimeNow;
                    data.UpdateDate = dateTimeNow;
                    data.UserSign2 = userId;
                    _dbContext.AttendanceSummarys.Attach(data);
                    _dbContext.Entry(data).State = EntityState.Modified;
                }
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception)
            {
                if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Thêm mới chứng từ Xác nhận giờ công
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="lstEntity1"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddConfirmWorkingDay(ConfirmWorkingDays entity, IEnumerable<ConfirmWorkingDay1s> lstEntity1)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@Type", GlobalConstants.TABLE_CONFIRM_WORKING_DAY, DbType.String);
                    parameters.Add("@BranchId", entity.BranchId, DbType.Int32);
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, @BranchId, '', '', '')";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: parameters, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    await _dbContext.Database.BeginTransactionAsync();
                    isTrans = true;
                    entity.Id = await _dbContext.ConfirmWorkingDays.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.VoucherNo = voucherNo;
                    entity.DateTracking = dateTimeNow;
                    entity.CreateDate = dateTimeNow;
                    await _dbContext.ConfirmWorkingDays.AddAsync(entity);
                    // thêm chi tiết đề nghị nghỉ phép
                    foreach (var item in lstEntity1)
                    {
                        // kiểm tra dữ liệu công ông đây chốt chưa -> nếu chốt công không cho tạo
                        var checkLocked = await _dbContext.AttendanceSummarys.FirstOrDefaultAsync(m => m.EmployeeId == item.EmployeeId && m.IsDelete == false
                                                && m.IsLocked == true && m.Month == item.WorkingDate.Month
                                                && m.Year == item.WorkingDate.Year);
                        if (checkLocked != null)
                        {
                            response.status = StatusCodes.Status409Conflict;
                            response.message = $"Nhân viên {checkLocked.EmployeeCode} đã được khóa kỳ dữ liệu công tháng {checkLocked.Month} năm {checkLocked.Year}";
                            if (isTrans) await _dbContext.Database.RollbackTransactionAsync();
                            return response;
                        }
                        ConfirmWorkingDay1s entity1 = new ConfirmWorkingDay1s();
                        entity1.ConfirmWorkingDayId = entity.Id;
                        entity1.EmployeeId = entity.EmployeeId;
                        entity1.WorkingDate = item.WorkingDate;
                        entity1.FromTime = item.FromTime;
                        entity1.ToTime = item.ToTime;
                        entity1.Remark = item.Remark;
                        entity1.ShiftCode = item.ShiftCode;
                        entity1.StartTime = item.StartTime;
                        entity1.EndTime = item.EndTime;
                        entity1.StartBreakTime = item.StartBreakTime;
                        entity1.EndBreakTime = item.EndBreakTime;
                        entity1.TotalWorkingHours = item.TotalWorkingHours;
                        entity1.StartTimeActual = item.StartTimeActual;
                        entity1.EndTimeActual = item.EndTimeActual;
                        entity1.StartBreakTimeActual = item.StartBreakTimeActual;
                        entity1.EndBreakTimeActual = item.EndBreakTimeActual;
                        entity1.TotalWorkingHoursActual = item.TotalWorkingHoursActual;
                        entity1.TotalMissingHours = item.TotalMissingHours;
                        entity1.DateTracking = dateTimeNow;
                        entity1.UserSign = entity.UserSign;
                        await _dbContext.ConfirmWorkingDay1s.AddAsync(entity1);
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
        public async Task<ResponseModel> UpdateConfirmWorkingDay(ConfirmWorkingDays entity, IEnumerable<ConfirmWorkingDay1s> lstEntity1)
        {
            bool isTrans = false;
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.ConfirmWorkingDays.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                if (data.DateTracking != entity.DateTracking)
                {
                    response.status = StatusCodes.Status409Conflict;
                    response.message = MessageConstants.MESSAGE_DATA_CHECKING_MODIFIED;
                    return response;
                }
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                data.EmployeeId = entity.EmployeeId;
                data.EmployeeSignatureId = entity.EmployeeSignatureId;
                data.WorkingDate = entity.WorkingDate;
                data.DepartmentId = entity.DepartmentId;
                data.Remark = entity.Remark;
                data.Remark = entity.Remark;
                data.DateTracking = dateTimeNow;
                data.UpdateDate = dateTimeNow;
                data.UserSign2 = entity.UserSign2;
                await _dbContext.Database.BeginTransactionAsync();
                isTrans = true;
                _dbContext.ConfirmWorkingDays.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                // thêm chi tiết đề nghị nghỉ phép
                // bỏ dữ liệu củ đi
                var lstLeaveRequest1s = await _dbContext.ConfirmWorkingDay1s.Where(m => m.ConfirmWorkingDayId == data.Id).ToListAsync();
                if (!lstLeaveRequest1s.IsNullOrEmpty()) _dbContext.ConfirmWorkingDay1s.RemoveRange(lstLeaveRequest1s);
                foreach (var item in lstEntity1)
                {
                    ConfirmWorkingDay1s entity1 = new ConfirmWorkingDay1s();
                    entity1.ConfirmWorkingDayId = entity.Id;
                    entity1.EmployeeId = entity.EmployeeId;
                    entity1.WorkingDate = item.WorkingDate;
                    entity1.FromTime = item.FromTime;
                    entity1.ToTime = item.ToTime;
                    entity1.Remark = item.Remark;
                    entity1.ShiftCode = item.ShiftCode;
                    entity1.StartTime = item.StartTime;
                    entity1.EndTime = item.EndTime;
                    entity1.StartBreakTime = item.StartBreakTime;
                    entity1.EndBreakTime = item.EndBreakTime;
                    entity1.TotalWorkingHours = item.TotalWorkingHours;
                    entity1.StartTimeActual = item.StartTimeActual;
                    entity1.EndTimeActual = item.EndTimeActual;
                    entity1.StartBreakTimeActual = item.StartBreakTimeActual;
                    entity1.EndBreakTimeActual = item.EndBreakTimeActual;
                    entity1.TotalWorkingHoursActual = item.TotalWorkingHoursActual;
                    entity1.TotalMissingHours = item.TotalMissingHours;
                    entity1.DateTracking = dateTimeNow;
                    entity1.UserSign = entity.UserSign;
                    await _dbContext.ConfirmWorkingDay1s.AddAsync(entity1);
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
        #endregion
    }
}
