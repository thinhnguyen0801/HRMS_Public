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
                        resUser.userId = response.data!.userId;
                        resUser.branchId = response.data.branchId;
                        resUser.branchCode = response.data.branchCode;
                        resUser.employeeId = response.data.employeeId;
                        resUser.employeeCode = response.data.employeeCode;
                        resUser.employeeName = response.data.employeeName;
                        resUser.token = response.data.token;
                        resUser.refreshToken = response.data.refreshToken;
                        resUser.isAdmin = response.data.isAdmin;
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
        public async Task<List<UserModel>?> GetUserAsync(RequestModel request)
        {
            try
            {
                List<UserModel>? data = null;
                request.process = ProcessConstants.GET_USER;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.USER_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<UserModel>>();
                    if (response == null || response.status != StatusCodes.Status200OK)
                    {
                        _toastService.ShowWarning(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        return data;
                    }
                    data = response.data?.ToList();
                }
                return data;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// lấy danh sách nhóm quyền
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<PermissionGroupModel>?> GetPermissionGroup(RequestModel request)
        {
            try
            {
                List<PermissionGroupModel>? data = null;
                request.process = ProcessConstants.GET_PER_GROUP;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.USER_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<PermissionGroupModel>>();
                    if (response == null || response.status != StatusCodes.Status200OK)
                    {
                        _toastService.ShowWarning(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        return data;
                    }
                    data = response.data?.ToList();
                }
                return data;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> UpdateUserAsync(string processKey, int userId, string token, string json)
        {
            try
            {
                RequestModel request = new RequestModel();
                request.process = processKey;
                request.userId = userId;
                request.token = token;
                request.json = json;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.USER_POST_DATA, request);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(MessageConstants.MESSAGE_LOGIN_EXPIRED);
                    return false;
                }
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowError(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var content = await httpResponse.Content.ReadAsStringAsync();
                    ResponseModel response = JsonConvert.DeserializeObject<ResponseModel>(content)!;
                    if (httpResponse.IsSuccessStatusCode
                        && response?.status == StatusCodes.Status200OK)
                    {
                        _toastService.ShowSuccess(response.message);
                        return true;
                    }
                    if (response?.status == StatusCodes.Status409Conflict)
                    {
                        _toastService.ShowInfo(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        return false;
                    }
                    _toastService.ShowError(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                }
                return false;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> UpdatePermissionGroupAsync(string processKey, int userId, string token, string json)
        {
            try
            {
                RequestModel request = new RequestModel();
                request.process = processKey;
                request.userId = userId;
                request.token = token;
                request.json = json;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.USER_POST_DATA, request);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(MessageConstants.MESSAGE_LOGIN_EXPIRED);
                    return false;
                }
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowError(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var content = await httpResponse.Content.ReadAsStringAsync();
                    ResponseModel response = JsonConvert.DeserializeObject<ResponseModel>(content)!;
                    if (httpResponse.IsSuccessStatusCode
                        && response?.status == StatusCodes.Status200OK)
                    {
                        _toastService.ShowSuccess(response.message);
                        return true;
                    }
                    _toastService.ShowError(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                }
                return false;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// cập nhật thông tin phân quyền theo nhóm
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePerGroupControlAsync(int groupId,int userId, string token, string json)
        {
            try
            {
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.POST_PER_GROUP_ACCESS_CONTROL;
                request.userId = userId;
                request.token = token;
                request.json = json;
                request.documentId = groupId;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.USER_POST_DATA, request);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(MessageConstants.MESSAGE_LOGIN_EXPIRED);
                    return false;
                }
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowError(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var content = await httpResponse.Content.ReadAsStringAsync();
                    ResponseModel response = JsonConvert.DeserializeObject<ResponseModel>(content)!;
                    if (httpResponse.IsSuccessStatusCode
                        && response?.status == StatusCodes.Status200OK)
                    {
                        _toastService.ShowSuccess(response.message);
                        return true;
                    }
                    _toastService.ShowError(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                }
                return false;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// lấy danh sách nhóm quyền
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<T>?> GetMasterDataAsync<T>(RequestModel request, bool isShowToast = false) where T : class
        {
            try
            {
                List<T>? data = null;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.USER_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<T>>();
                    if (response == null || response.status != StatusCodes.Status200OK)
                    {
                        if(isShowToast) _toastService.ShowWarning(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        return data;
                    }
                    data = response.data?.ToList();
                }
                return data;
            }
            catch (Exception) { throw; }
        }
    }
}
