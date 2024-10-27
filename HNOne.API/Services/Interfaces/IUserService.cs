using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<ResponseModel<UserModel>> Login(LoginRequestModel request);
        Task UpdateRefreshToken(int userId, string token, int refreshTokenExpiryTime);
    }
}
