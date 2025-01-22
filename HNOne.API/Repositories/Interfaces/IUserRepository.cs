using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;

namespace HNOne.API.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<ResponseModel<UserModel>> Login(LoginRequestModel request);
        Task UpdateRefreshToken(int userId, string token, int refreshTokenExpiryTime);
        Task<IEnumerable<UserModel>> GetUser(RequestModel request);
        Task<ResponseModel> AddUser(Users entity);
        Task<ResponseModel> UpdateUser(Users entity);
        Task<IEnumerable<PermissionGroupModel>> GetPermissionGroup(RequestModel request);
        Task<ResponseModel> AddPermissionGroup(PermissionGroups entity);
        Task<ResponseModel> UpdatePermissionGroup(PermissionGroups entity);
        Task<ResponseModel> UpdateGroupAccessControl(int groupId, IEnumerable<GroupAccessControls> listEntity, IEnumerable<DataPermissions> lstAuthData);
        Task<IEnumerable<GroupAccessControls>> GetPermissionByGroupId(int groupId);
        Task<IEnumerable<MenuModel>> GetDataPermissionByGroupId(int groupId);
    }
}
