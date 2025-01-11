using Azure;
using Azure.Core;
using Dapper;
using HNOne.API.Constants;
using HNOne.API.Repositories.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using System.Net;
using static Dapper.SqlMapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HNOne.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MasterDbContext _dbContext;
        private readonly IDapperDbContext _dapperDbContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEncryptHelper _encryptHelper;
        public UserRepository(MasterDbContext dbContext, IDapperDbContext dapperDbContext
            , IDateTimeHelper dateTimeHelper, IEncryptHelper encryptHelper)
        {
            _dbContext = dbContext;
            _dapperDbContext = dapperDbContext;
            _dateTimeHelper = dateTimeHelper;
            _encryptHelper = encryptHelper;
        }

        /// <summary>
        /// đăng nhập
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseModel<UserModel>> Login(LoginRequestModel request)
        {
            ResponseModel<UserModel> response = new ResponseModel<UserModel>();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    DynamicParameters parameters = new DynamicParameters();
                    bool isResult = true;
                    // kiểm tra tên đăng nhập
                    string userName = _encryptHelper.Decrypt(request.userName);
                    string strQuery = "select count(1) from [Users] with(nolock) where UserName = @UserName";
                    isResult = await connection.ExecuteScalarAsync<bool>(strQuery, new { userName });
                    if (!isResult)
                    {
                        response.status = StatusCodes.Status404NotFound;
                        response.message = "Tên đăng nhập không hợp lệ!";
                        return response;
                    }
                    // Kiểm tra đăng nhập đúng chi nhánh không?
                    strQuery = "select count(1) from [Users] with(nolock) where UserName = @UserName and charindex(','+ @BranchId +',',','+ BranchIds +',') > 0";
                    parameters = new DynamicParameters();
                    parameters.Add("@UserName", userName, DbType.String);
                    parameters.Add("@BranchId", request.branchId, DbType.String);
                    isResult = await connection.ExecuteScalarAsync<bool>(strQuery, parameters);
                    if (!isResult)
                    {
                        response.status = StatusCodes.Status404NotFound;
                        response.message = "Bạn không thuộc chi nhánh được chọn. Vui lòng liên hệ IT để được hổ trợ!";
                        return response;
                    }

                    // Kiểm tra đúng password chưa
                    strQuery = "Select T0.UserId, T0.UserName, T0.EmployeeId, T0.DepartmentIds, T0.BranchIds, T0.IsAdmin, T0.IsActive, T0.IsDelete" +
                        " , T1.BranchId, T1.BranchCode, T1.BranchName" +
                        " , T2.[Code] EmployeeCode,T2.[Name] EmployeeName, T2.DepartmentId" +
                        " from [Users] as T0 with(nolock)" +
                        " inner join Employees as T2 with(nolock) on T0.EmployeeId = T2.Id" +
                        " cross apply (select T00.BranchId, T00.BranchCode, T00.BranchName from Branchs as T00 with(nolock) where T00.BranchId = @BranchId ) as T1" +
                        " where UserName = @UserName and (T0.Password = @Password or T0.DefaultPassword = @Password)";
                    parameters = new DynamicParameters();
                    parameters.Add("@UserName", userName, DbType.String);
                    parameters.Add("@Password", request.password, DbType.String);
                    parameters.Add("@BranchId", request.branchId, DbType.String);
                    var result = await connection.QueryFirstOrDefaultAsync<UserModel>(strQuery, parameters, commandTimeout: 500, commandType: CommandType.Text);
                    if (result == null)
                    {
                        response.status = StatusCodes.Status404NotFound;
                        response.message = "Mật khẩu không hợp lệ!";
                        return response;
                    }
                    if(result.isDelete || !result.isActive)
                    {
                        response.status = StatusCodes.Status404NotFound;
                        response.message = "Tài khoản đã bị khóa. Vui lòng liên hệ IT để được hổ trợ!";
                        return response;
                    }    
                    response.status = StatusCodes.Status200OK;
                    response.data = result;
                }
            }
            catch (Exception ex)
            {
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;

        }

        /// <summary>
        /// cập nhật refresh token
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="refreshTokenExpiryTime"></param>
        /// <returns></returns>
        public async Task UpdateRefreshToken(int userId, string token, int refreshTokenExpiryTime)
        {
            try
            {
                var data = await _dbContext.Users.FirstOrDefaultAsync(m => m.UserId == userId);
                if (data == null) return;
                data.RefreshToken = token;
                data.RefreshTokenExpiryTime = _dateTimeHelper.GetCurrentVietnamTime().AddDays(refreshTokenExpiryTime);
                _dbContext.Users.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
            catch(Exception) { throw; }
        }

        /// <summary>
        /// lấy danh sách tài khoản
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<UserModel>> GetUser(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                DynamicParameters parameters = new DynamicParameters();
                string strQuery = "select T0.*" +
                    " ,T1.BranchCode as BranchCode, T1.BranchName as BranchName, T2.[Code] as EmployeeCode, T2.[Name] as EmployeeName" +
                    " ,T3.[Code] as PerGroupCode, T3.[Name] as PerGroupName" +
                    " from Users as T0 with(nolock) " +
                    " inner join Branchs as T1 with(nolock) on T0.BranchId = T1.BranchId " +
                    " left join Employees as T2 with(nolock) on T0.EmployeeId = T2.Id" +
                    " left join PermissionGroups as T3 with(nolock) on T0.PerGroupId = T3.Id" +
                    " where T0.IsDelete = 0";
                if (request.documentId > 0)
                {
                    strQuery += " and T0.UserId = @UserId";
                    parameters.Add("@UserId", request.documentId, DbType.Int32);
                }
                var result = await connection.QueryAsync<UserModel>(strQuery, parameters, commandTimeout: 500, commandType: CommandType.Text);
                return result;
            }
;
        }

        /// <summary>
        /// lấy danh sách Model
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PermissionGroupModel>> GetPermissionGroup(RequestModel request)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string strQuery = "select T0.*" +
                    " from PermissionGroups as T0 with(nolock)" +
                    " where IsDelete = 0";
                // thêm điều kiện
                if (request.opt == CommonConstants.ENUM_ACTIVE) strQuery += " and T0.IsActive = '1'";
                var result = await connection.QueryAsync<PermissionGroupModel>(strQuery, commandTimeout: 500, commandType: CommandType.Text);
                return result;
            }    
        }

        public async Task<IEnumerable<GroupAccessControls>> GetPermissionByGroupId(int groupId)
        {
            using (var connection = _dapperDbContext.CreateConnection())
            {
                string strQuery = "select T0.*" +
                    " from GroupAccessControls as T0 with(nolock)" +
                    " where IsDelete = 0 and T0.GroupId = @GroupId";
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@GroupId", groupId, DbType.Int32);
                var result = await connection.QueryAsync<GroupAccessControls>(strQuery, parameters, commandTimeout: 500, commandType: CommandType.Text);
                return result;
            }
        }
        #region Command
        /// <summary>
        /// Thêm mới tài khoản mới
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddUser(Users entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    bool isResult = true;
                    // Tạo mới
                    isResult = await _dbContext.Users.FirstOrDefaultAsync(m => m.UserName == entity.UserName) != null;
                    if (isResult)
                    {
                        response.status = StatusCodes.Status409Conflict;
                        response.message = "Tên đăng nhập đã tồn tại!";
                        return response;
                    }
                    if(entity.EmployeeId > 0)
                    {
                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@EmployeeId", entity.EmployeeId, DbType.Int32);
                        string strQuery = "select T0.*, T2.[Code] as EmployeeCode, T2.[Name] as EmployeeName" +
                            " from Users as T0 with(nolock) " +
                            " inner join Employees as T2 with(nolock) on T0.EmployeeId = T2.Id" +
                            " where T0.IsDelete = 0 and T0.IsActive = 1 and T0.EmployeeId = @EmployeeId";
                        var result = await connection.QueryFirstOrDefaultAsync<UserModel>(strQuery, parameters, commandTimeout: 500, commandType: CommandType.Text);
                        if (result != null)
                        {
                            response.status = StatusCodes.Status409Conflict;
                            response.message = $"Nhân viên [{result.employeeCode}] đã thiết lập tài khoản [{result.userName}]!";
                            return response;
                        }
                    }    
                    entity.UserId = await _dbContext.Users.Select(m => m.UserId).DefaultIfEmpty().MaxAsync() + 1;
                    entity.Password = _encryptHelper.Encrypt(entity.Password?.Trim());
                    entity.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    entity.CreateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    await _dbContext.Users.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                }
                return response;
            }
            catch (Exception) { throw; }

        }

        /// <summary>
        /// cập nhật tài khoản
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateUser(Users entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {

                using (var connection = _dapperDbContext.CreateConnection())
                {
                    var data = await _dbContext.Users.FirstOrDefaultAsync(m => m.UserId == entity.UserId);
                    if (data == null)
                    {
                        response.status = StatusCodes.Status404NotFound;
                        response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                        return response;
                    }
                    if (entity.EmployeeId > 0)
                    {
                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@EmployeeId", entity.EmployeeId, DbType.Int32);
                        parameters.Add("@UserId", entity.UserId, DbType.Int32);
                        string strQuery = "select T0.*, T2.[Code] as EmployeeCode, T2.[Name] as EmployeeName" +
                            " from Users as T0 with(nolock) " +
                            " inner join Employees as T2 with(nolock) on T0.EmployeeId = T2.Id" +
                            " where T0.IsDelete = 0 and T0.IsActive = 1 and T0.EmployeeId = @EmployeeId and T0.UserId <> @UserId";
                        var result = await connection.QueryFirstOrDefaultAsync<UserModel>(strQuery, parameters, commandTimeout: 500, commandType: CommandType.Text);
                        if (result != null)
                        {
                            response.status = StatusCodes.Status409Conflict;
                            response.message = $"Nhân viên [{result.employeeCode}] đã thiết lập tài khoản [{result.userName}]!";
                            return response;
                        }
                    }
                    data.Password = _encryptHelper.Encrypt(entity.Password);
                    data.BranchId = entity.BranchId;
                    data.EmployeeId = entity.EmployeeId;
                    data.DepartmentIds = entity.DepartmentIds;
                    data.BranchIds = entity.BranchIds;
                    data.IsActive = entity.IsActive;
                    data.IsAdmin = entity.IsAdmin;
                    data.PerGroupId = entity.PerGroupId;
                    data.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                    data.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                    data.UserSign2 = entity.UserSign2;
                    _dbContext.Users.Attach(data);
                    _dbContext.Entry(data).State = EntityState.Modified;
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                    return response;
                }    
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Thêm mới thông tin nhóm quyền
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddPermissionGroup(PermissionGroups entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var connection = _dapperDbContext.CreateConnection())
                {
                    DateTime currentDate = _dateTimeHelper.GetCurrentVietnamTime();
                    string commandText = @$"select {StoreConstants.FUNC_GET_VOUCHER}(@Type, '', '', '')";
                    string? voucherNo = await connection.QueryFirstOrDefaultAsync<string>(commandText, param: new { Type = GlobalConstants.TABLE_PERMISSION_GROUP }, commandTimeout: GlobalConstants.COMMAND_TIMEOUT, commandType: CommandType.Text);
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        response.status = StatusCodes.Status204NoContent;
                        response.message = MessageConstants.MESSAGE_VOUCHER_NO_MISSING;
                        return response;
                    }
                    entity.Id = await _dbContext.PermissionGroups.Select(m => m.Id).DefaultIfEmpty().MaxAsync() + 1;
                    entity.Code = voucherNo;
                    entity.DateTracking = currentDate;
                    entity.CreateDate = currentDate;
                    await _dbContext.PermissionGroups.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    response.message = MessageConstants.MESSAGE_ADD_SUCCESS;
                }
                return response;
            }
            catch (Exception) { throw; }

        }

        /// <summary>
        /// cập nhật thông tin nhóm quyền
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdatePermissionGroup(PermissionGroups entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                var data = await _dbContext.PermissionGroups.FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (data == null)
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_NOT_FOUNT;
                    return response;
                }
                data.Name = entity.Name;
                data.IsActive = entity.IsActive;
                data.Remark = entity.Remark;
                data.DateTracking = _dateTimeHelper.GetCurrentVietnamTime();
                data.UpdateDate = _dateTimeHelper.GetCurrentVietnamTime();
                data.UserSign2 = entity.UserSign2;
                _dbContext.PermissionGroups.Attach(data);
                _dbContext.Entry(data).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
                return response;
            }
            catch (Exception) { throw; }
        }
        
        /// <summary>
        /// cập nhật thông tin nhóm quyền
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="listEntity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateGroupAccessControl(int groupId,IEnumerable<GroupAccessControls> listEntity)
        {
            ResponseModel response = new ResponseModel();
            bool isTran = false;
            try
            {
                DateTime dateTimeNow = _dateTimeHelper.GetCurrentVietnamTime();
                await _dbContext.Database.BeginTransactionAsync();
                isTran = true;
                foreach (var entity in listEntity)
                {
                    var checkUpdate = await _dbContext.GroupAccessControls.FirstOrDefaultAsync(m => m.GroupId == groupId && m.EventId == entity.EventId);
                    if (checkUpdate == null)
                    {
                        entity.GroupId = groupId;
                        entity.DateTracking = dateTimeNow;
                        entity.CreateDate = dateTimeNow;
                        await _dbContext.GroupAccessControls.AddAsync(entity);
                        continue;
                    }
                    checkUpdate.IsAllow = entity.IsAllow;
                    checkUpdate.DateTracking = dateTimeNow;
                    checkUpdate.UpdateDate = dateTimeNow;
                    checkUpdate.UserSign2 = entity.UserSign2;
                    _dbContext.GroupAccessControls.Attach(checkUpdate);
                    _dbContext.Entry(checkUpdate).State = EntityState.Modified;
                }
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
                response.message = MessageConstants.MESSAGE_UPDATE_SUCCESS;
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
    }
}
