using Blazored.Toast.Services;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Services.Interfaces;
using Newtonsoft.Json;

namespace HNOne.Web.Services
{
    public class SalaryService : ApiServiceBase, ISalaryService
    {
        private IToastService _toastService { get; init; }
        public SalaryService(IHttpClientFactory factory, ILogger<SalaryService> logger, IToastService toastService)
            : base(factory, logger)
        {
            _toastService = toastService;
        }

        /// <summary>
        /// lấy danh sách chung
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<T>?> GetMasterDataAsync<T>(RequestModel request, bool isShowToast = false) where T : class
        {

            try
            {
                List<T>? data = null;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.SALARY_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<T>>();
                    if (response == null || response.status != StatusCodes.Status200OK)
                    {
                        if (isShowToast) _toastService.ShowWarning(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        return data;
                    }
                    data = response.data?.ToList();
                }
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMenuAsync");
                throw;
            }
        }

        /// <summary>
        /// cập nhật dữ liệu chung
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> UpdateMasterDataAsync(RequestModel request)
        {
            try
            {
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.SALARY_POST_DATA, request);
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
    
        /// <summary>
        /// Tính lương nhân viên
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<PayrollModel>?> SalaryCalculateAsync(RequestModel request)
        {
            try
            {
                List<PayrollModel>? data = null;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.SALARY_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<PayrollModel>>();
                    if (httpResponse.IsSuccessStatusCode
                        && response?.status == StatusCodes.Status200OK
                        && !response.data.IsNullOrEmpty())
                    {
                        var header = response.data!.First();
                        if (header.status == StatusCodes.Status409Conflict)
                        {
                            _toastService.ShowInfo(header.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                            return data;
                        }
                        data = response.data?.ToList();
                        return data;
                    }
                    _toastService.ShowWarning(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                }
                return data;
            }
            catch (Exception) { throw; }
        }
    
        /// <summary>
        /// lưu thông tin bảng lương của nhân viên
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<string> UpdatePayrollAsync(RequestModel request)
        {
            string statusId = "-1";
            try
            {
                request.process = ProcessConstants.POST_PAYROLL_SALARY;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.SALARY_POST_DATA, request);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(MessageConstants.MESSAGE_LOGIN_EXPIRED);
                    return statusId;
                }
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent)
                {
                    _toastService.ShowError(MessageConstants.MESSAGE_JSON_INVALID);
                    return statusId;
                }
                var content = await httpResponse.Content.ReadAsStringAsync();
                ResponseModel response = JsonConvert.DeserializeObject<ResponseModel>(content)!;
                if (httpResponse.IsSuccessStatusCode
                    && response.status == StatusCodes.Status200OK)
                {
                    _toastService.ShowSuccess(response.message);
                    return "200"; // -- thành công
                }
                // Xử lý mã trạng thái cụ thể
                switch (response?.status)
                {
                    case StatusCodes.Status409Conflict:
                        _toastService.ShowInfo(response.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        break;
                    case -StatusCodes.Status409Conflict:
                        return response.message ; // để hiện thị cái popup confirm
                    default:
                        _toastService.ShowError(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        break;
                }

                return statusId;
            }
            catch (Exception) { throw; }
        }
    
        /// <summary>
        /// Cập nhật thông tin document
        /// </summary>
        /// <param name="processKey"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="branchId"></param>
        /// <param name="json"></param>
        /// <param name="jsonDetail"></param>
        /// <returns></returns>
        public async Task<int> UpdateDocumentAsync(string processKey, int userId, string token, int branchId, string json, string jsonDetail, bool isShowToast = true)
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
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.SALARY_POST_DATA, request);
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
                        if (isShowToast) _toastService.ShowSuccess(response.message);
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
    }
}
