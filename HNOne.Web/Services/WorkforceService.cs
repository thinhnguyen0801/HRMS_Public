using Blazored.Toast.Services;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Services.Interfaces;
using Newtonsoft.Json;

namespace HNOne.Web.Services
{
    public class WorkforceService : ApiServiceBase, IWorkforceService
    {
        private IToastService _toastService { get; init; }
        public WorkforceService(IHttpClientFactory factory, ILogger<WorkforceService> logger, IToastService toastService)
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
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.WORKFORCE_GET_DATA, request);
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
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.WORKFORCE_POST_DATA, request);
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
        /// lấy danh sách/chi tiết đề nghị nghỉ phép
        /// </summary>
        /// <param name="request"></param>
        /// <param name="isShowToast"></param>
        /// <returns></returns>
        public async Task<List<LeaveRequestModel>?> GetLeaveRequestAsync(RequestModel request, bool isShowToast = false)
        {
            try
            {
                List<LeaveRequestModel>? data = null;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.WORKFORCE_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<LeaveRequestModel>>();
                    if (response == null || response.status != StatusCodes.Status200OK)
                    {
                        if (isShowToast) _toastService.ShowWarning(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        return data;
                    }
                    data = response.data?.ToList();
                }
                return data;
            }
            catch (Exception) { throw; }
        }

        public async Task<int> UpdateLeaveRequestAsync(string processKey, int userId, string token, int branchId, string json, string jsonDetail, bool isShowToast = true)
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
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.WORKFORCE_POST_DATA, request);
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

        /// <summary>
        /// lấy danh sách/chi tiết đề nghị nghỉ phép
        /// </summary>
        /// <param name="request"></param>
        /// <param name="isShowToast"></param>
        /// <returns></returns>
        public async Task<List<ShiftChangeModel>?> GetShiftChangeRequestAsync(RequestModel request, bool isShowToast = false)
        {
            try
            {
                List<ShiftChangeModel>? data = null;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.WORKFORCE_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<ShiftChangeModel>>();
                    if (response == null || response.status != StatusCodes.Status200OK)
                    {
                        if (isShowToast) _toastService.ShowWarning(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        return data;
                    }
                    data = response.data?.ToList();
                }
                return data;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// lấy danh sách/chi tiết đề nghị nghỉ phép
        /// </summary>
        /// <param name="request"></param>
        /// <param name="isShowToast"></param>
        /// <returns></returns>
        public async Task<List<OvertimeRequestModel>?> GetOvertimeRequestAsync(RequestModel request, bool isShowToast = false)
        {
            try
            {
                List<OvertimeRequestModel>? data = null;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.WORKFORCE_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<OvertimeRequestModel>>();
                    if (response == null || response.status != StatusCodes.Status200OK)
                    {
                        if (isShowToast) _toastService.ShowWarning(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        return data;
                    }
                    data = response.data?.ToList();
                }
                return data;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Preview hoặc phát sinh công chi tiết
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<WorkConfigModel>?> GenerateWorkConfigAsync(RequestModel request)
        {
            try
            {
                List<WorkConfigModel>? data = null;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.WORKFORCE_POST_DATA, request);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(MessageConstants.MESSAGE_LOGIN_EXPIRED);
                    return default;
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
                        if(request.type == "PREVIEW")
                        {
                            data = JsonConvert.DeserializeObject<List<WorkConfigModel>>($"{response.returnValue}");
                            return data;
                        }    
                        _toastService.ShowSuccess(response.message);
                    }
                    if (response?.status == StatusCodes.Status409Conflict)
                    {
                        _toastService.ShowInfo(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        return data;
                    }
                    _toastService.ShowError(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                }
                return data;
            }
            catch (Exception) { throw; }
        }
    
        /// <summary>
        /// lấy dữ liệu tính công nhân viên
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<ShiftAssignmentModel>?> WorkCalculateAsync(RequestModel request)
        {
            try
            {
                List<ShiftAssignmentModel>? data = null;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.WORKFORCE_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<ShiftAssignmentModel>>();
                    if (httpResponse.IsSuccessStatusCode
                        && response?.status == StatusCodes.Status200OK
                        && !response.data.IsNullOrEmpty())
                    {
                        var header = response.data!.First();
                        if(header.status == StatusCodes.Status409Conflict)
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
        /// lấy danh sách/chi tiết điều chỉnh phép nắm
        /// </summary>
        /// <param name="request"></param>
        /// <param name="isShowToast"></param>
        /// <returns></returns>
        public async Task<List<AdjustedAnnualLeaveRequestModel>?> GetAdjustedALRequestAsync(RequestModel request, bool isShowToast = false)
        {
            try
            {
                List<AdjustedAnnualLeaveRequestModel>? data = null;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.WORKFORCE_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<AdjustedAnnualLeaveRequestModel>>();
                    if (response == null || response.status != StatusCodes.Status200OK)
                    {
                        if (isShowToast) _toastService.ShowWarning(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        return data;
                    }
                    data = response.data?.ToList();
                }
                return data;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// lấy danh sách/chi tiết chứng từ đề nghị
        /// </summary>
        /// <param name="request"></param>
        /// <param name="isShowToast"></param>
        /// <returns></returns>
        public async Task<List<DecisionDocumentModel>?> GetDecisionDocumentAsync(RequestModel request, bool isShowToast = false)
        {
            try
            {
                List<DecisionDocumentModel>? data = null;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.WORKFORCE_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<DecisionDocumentModel>>();
                    if (response == null || response.status != StatusCodes.Status200OK)
                    {
                        if (isShowToast) _toastService.ShowWarning(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
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
