using Blazored.Toast.Services;
using HNOne.Common;
using HNOne.Model;
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
                    _toastService.ShowError(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                }
                return false;
            }
            catch (Exception) { throw; }
        }
    }
}
