using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<ResponseModel<UserModel>> LoginAsync(LoginRequestModel request);
    }
}
