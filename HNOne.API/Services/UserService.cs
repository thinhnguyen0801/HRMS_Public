using HNOne.API.Services.Interfaces;
using HNOne.Model.Models;
using HNOne.Model;
using HNOne.API.Repositories.Interfaces;

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
    }
}
