using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;

namespace HNOne.API.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<ResponseModel<UserModel>> Login(LoginRequestModel request);
        Task UpdateRefreshToken(int userId, string token, int refreshTokenExpiryTime);
    }
}
