using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;

namespace HNOne.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<ResponseModel<UserModel>> Login(LoginRequestModel request);
        Task UpdateRefreshToken(int userId, string token, int refreshTokenExpiryTime);
        Task<IEnumerable<UserModel>> GetUser(RequestModel request);
        Task<ResponseModel> UpdateUser(string actionType, Users entity);

    }
}
