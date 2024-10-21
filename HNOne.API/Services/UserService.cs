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

        public Task<ResponseModel<UserModel>> LoginAsync(LoginRequestModel request)
        {
            throw new NotImplementedException();
        }
    }
}
