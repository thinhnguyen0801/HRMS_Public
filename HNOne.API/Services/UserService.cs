using HNOne.API.Services.Interfaces;
using HNOne.Model.Models;
using HNOne.Model;
using HNOne.API.Repositories.Interfaces;
using HNOne.API.Repositories;
using HNOne.Common;
using HNOne.Model.Entities;

namespace HNOne.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ResponseModel<UserModel>> Login(LoginRequestModel request)
            => await _userRepository.Login(request);

        public async Task UpdateRefreshToken(int userId, string token, int refreshTokenExpiryTime)
            => await _userRepository.UpdateRefreshToken(userId, token, refreshTokenExpiryTime);
        public async Task<IEnumerable<UserModel>> GetUser(RequestModel request)
            => await _userRepository.GetUser(request);

        public async Task<IEnumerable<PermissionGroupModel>> GetPermissionGroup(RequestModel request)
            => await _userRepository.GetPermissionGroup(request);

        public async Task<IEnumerable<GroupAccessControls>> GetPermissionByGroupId(int groupId)
            => await _userRepository.GetPermissionByGroupId(groupId);
        public async Task<IEnumerable<MenuModel>> GetDataPermissionByGroupId(int groupId)
            => await _userRepository.GetDataPermissionByGroupId(groupId);
        #region Command
        public async Task<ResponseModel> UpdateUser(string actionType, Users entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_USER:
                        response = await _userRepository.AddUser(entity);
                        break;
                    case ProcessConstants.PUT_USER:
                        response = await _userRepository.UpdateUser(entity);
                        break;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// cập nhật thông tin nhóm quyền
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdatePermissionGroup(string actionType, PermissionGroups entity)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                switch (actionType)
                {
                    case ProcessConstants.POST_PER_GROUP:
                        response = await _userRepository.AddPermissionGroup(entity);
                        break;
                    case ProcessConstants.PUT_PER_GROUP:
                        response = await _userRepository.UpdatePermissionGroup(entity);
                        break;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel> UpdateGroupAccessControl(int groupId, IEnumerable<GroupAccessControls> listEntity, IEnumerable<DataPermissions> lstAuthData)
            => await _userRepository.UpdateGroupAccessControl(groupId, listEntity, lstAuthData);
        
        public async Task<ResponseModel> UpdatePassword(UserModel request)
            => await _userRepository.UpdatePassword(request);
        #endregion
    }
}
