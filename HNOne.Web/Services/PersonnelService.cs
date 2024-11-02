using Blazored.Toast.Services;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Services.Interfaces;
using Newtonsoft.Json;

namespace HNOne.Web.Services
{
    public class PersonnelService : ApiServiceBase, IPersonnelService
    {
        private IToastService _toastService { get; init; }
        public PersonnelService(IHttpClientFactory factory, ILogger<PersonnelService> logger, IToastService toastService)
            : base(factory, logger)
        {
            _toastService = toastService;
        }
        public async Task<List<EmployeeModel>?> GetEmployeeAsync(RequestModel request)
        {
            try
            {
                List<EmployeeModel>? data = null;
                request.process = ProcessConstants.GET_EMPLOYEE;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.PERSONNEL_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<EmployeeModel>>();
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

        public async Task<bool> UpdateEmployeeAsync(string processKey, int userId, string token, string json)
        {
            try
            {
                RequestModel request = new RequestModel();
                request.process = processKey;
                request.userId = userId;
                request.token = token;
                request.json = json;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.PERSONNEL_POST_DATA, request);
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
        /// cập nhật thông tin hợp đồng
        /// </summary>
        /// <param name="processKey"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="branchId"></param>
        /// <param name="json"></param>
        /// <param name="jsonDetail"></param>
        /// <returns></returns>
        public async Task<int> UpdateContractAsync(string processKey, int userId, string token, int branchId, string json, string jsonDetail)
        {
            try
            {
                RequestModel request = new RequestModel();
                request.process = processKey;
                request.userId = userId;
                request.token = token;
                request.branchId = branchId;
                request.json = json;
                request.jsonDetail = jsonDetail;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.PERSONNEL_POST_DATA, request);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(MessageConstants.MESSAGE_LOGIN_EXPIRED);
                    return -1;
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
                        int.TryParse(response.data?.ToString(), out int result);
                        return result;
                    }
                    if (response?.status == StatusCodes.Status409Conflict)
                    {
                        _toastService.ShowInfo(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        return -1;
                    }
                    _toastService.ShowError(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                }
                return -1;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// lấy danh sách hợp đồng
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<ContractModel>?> GetContractAsync(RequestModel request, bool isShowToast = false)
        {
            try
            {
                List<ContractModel>? data = null;
                request.process = ProcessConstants.GET_CONTRACT;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.PERSONNEL_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<ContractModel>>();
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
