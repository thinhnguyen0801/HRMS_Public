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
        #endregion
    }
}
