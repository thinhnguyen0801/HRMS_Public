using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.Web.Services.Interfaces
{
    public interface IUserService
    {
        Task<string> LoginAsync(LoginRequestModel request);
        Task<List<UserModel>?> GetUserAsync(RequestModel request);
        Task<bool> UpdateUserAsync(string processKey, int userId, string token, string json);
        Task<List<PermissionGroupModel>?> GetPermissionGroup(RequestModel request);
        Task<bool> UpdatePermissionGroupAsync(string processKey, int userId, string token, string json);
        Task<bool> UpdatePerGroupControlAsync(int groupId, int userId, string token, string json);
        Task<List<T>?> GetMasterDataAsync<T>(RequestModel request, bool isShowToast = false) where T : class;
    }
}
