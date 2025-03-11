using Blazored.Toast.Services;
using HNOne.Common;
using HNOne.Model;
using HNOne.Web.Commons;
using HNOne.Web.Services.Interfaces;

namespace HNOne.Web.Services
{
    public class ReportService : ApiServiceBase, IReportService
    {
        private IToastService _toastService { get; init; }
        public ReportService(IHttpClientFactory factory, ILogger<ReportService> logger, IToastService toastService)
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
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.REPORT_GET_DATA, request);
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
                _logger.LogError(ex, "GetMasterDataAsync");
                throw;
            }
        }
    }
}
