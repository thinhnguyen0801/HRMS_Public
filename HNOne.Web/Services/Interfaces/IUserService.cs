using HNOne.Model;

namespace HNOne.Web.Services.Interfaces
{
    public interface IUserService
    {
        Task<string> LoginAsync(LoginRequestModel request);
    }
}
