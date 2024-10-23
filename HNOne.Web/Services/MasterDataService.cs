using Blazored.Toast.Services;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Services.Interfaces;
using Newtonsoft.Json;

namespace HNOne.Web.Services
{
    public class MasterDataService : ApiServiceBase, IMasterDataService
    {
        private IToastService _toastService { get; init; }
        public MasterDataService(IHttpClientFactory factory, ILogger<MasterDataService> logger, IToastService toastService) 
            : base(factory, logger)
        {
            _toastService = toastService;
        }

        /// <summary>
        /// lấy danh sách menu
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<Menus>?> GetMenuAsync(RequestModel request)
        {
            
            try
            {
                List<Menus>? data = null;
                request.process = ProcessConstants.GET_MENU;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<Menus>>();
                    if (response == null || response.status != StatusCodes.Status200OK)
                    {
                        _toastService.ShowWarning(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        return data;
                    }
                    data = response.data?.ToList();
                }
                return data;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "GetMenuAsync");
                throw ex;
            }
        }

        /// <summary>
        /// lấy danh sách chi nhánh
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<BranchModel>?> GetBranchAsync(int userId, string token = "")
        {

            try
            {
                List<BranchModel>? data = null;
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_BRANCH;
                request.userId = userId;
                request.token = token;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA_WITHOUT_TOKEN, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<BranchModel>>();
                    if (response == null || response.status != StatusCodes.Status200OK)
                    {
                        _toastService.ShowWarning(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        return data;
                    }
                    data = response.data?.ToList();
                }
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetBranchAsync");
                throw ex;
            }
        }

        /// <summary>
        /// cập nhật thông tin chi nhánh
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="processKey"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public async Task<bool> UpdateBranchAsync(string processKey, int userId, string token, string json)
        {
            try
            {
                RequestModel request = new RequestModel();
                request.process = processKey;
                request.userId = userId;
                request.token = token;
                request.json = json;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_POST_DATA, request);
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
                return true;
            }
            catch (Exception) { throw; }
        }
    }
}
