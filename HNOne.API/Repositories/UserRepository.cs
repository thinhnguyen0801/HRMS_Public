using Azure;
using Dapper;
using HNOne.API.Repositories.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net;
using static Dapper.SqlMapper;

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
                    strQuery = "Select * from [Users] with(nolock) where UserName = @UserName "
                    + "and (Password = @Password or DefaultPassword = @Password)";
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
    }
}
