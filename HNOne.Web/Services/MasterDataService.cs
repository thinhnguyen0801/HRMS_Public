using Blazored.Toast.Services;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Web.Commons;
using HNOne.Web.Services.Interfaces;

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
        public async Task<List<Branchs>?> GetBranchAsync(RequestModel request)
        {

            try
            {
                List<Branchs>? data = null;
                request.process = ProcessConstants.GET_BRANCH;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA_WITHOUT_TOKEN, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<Branchs>>();
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
    }
}
