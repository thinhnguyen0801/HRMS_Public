using Blazored.Toast.Services;
using HNOne.Model;
using HNOne.Web.Services.Interfaces;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Common;
using Newtonsoft.Json;
using Blazored.LocalStorage;
using HNOne.Web.Models;
using HNOne.Model.Entities;
namespace HNOne.Web.Services
{
    public class UserService : ApiServiceBase, IUserService
    {
        private readonly IToastService _toastService;
        private readonly ILocalStorageService _localStorage;
        private readonly IEncryptHelper _encryptHelper;
        public UserService(IHttpClientFactory factory, ILogger<UserService> logger
            , IToastService toastService, ILocalStorageService localStorage, IEncryptHelper encryptHelper)
            : base(factory, logger)
        {
            _toastService = toastService;
            _localStorage = localStorage;
            _encryptHelper = encryptHelper;
        }

        public async Task<string> LoginAsync(LoginRequestModel request)
        {
            string errorMessage = "";
            try
            {
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.USER_LOGIN, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) errorMessage = MessageConstants.MESSAGE_JSON_INVALID;
                else
                {
                    var content = await httpResponse.Content.ReadAsStringAsync();
                    ResponseModel<UserModel> response = JsonConvert.DeserializeObject<ResponseModel<UserModel>>(content)!; ;
                    if (httpResponse.IsSuccessStatusCode
                            && response?.status == StatusCodes.Status200OK)
                    {
                        // save token
                        if (await _localStorage.ContainKeyAsync("authToken")) await _localStorage.RemoveItemAsync("authToken");
                        ResUserModel resUser = new ResUserModel();
                        resUser.branchId = response.data!.branchId;
                        resUser.branchCode = response.data!.branchCode;
                        resUser.employeeCode = response.data!.employeeCode;
                        resUser.employeeName = response.data!.employeeName;
                        resUser.token = response.data!.token;
                        resUser.refreshToken = response.data!.refreshToken;
                        resUser.isAdmin = response.data!.isAdmin;
                        string encryptUser = _encryptHelper.Encrypt(JsonConvert.SerializeObject(resUser));
                        await _localStorage.SetItemAsync("authToken", encryptUser);
                        return "";
                    }

                    errorMessage = response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT;
                }
                return errorMessage;
            }
            catch (Exception){ throw; }
        }
    }
}
