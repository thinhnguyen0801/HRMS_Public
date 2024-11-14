using Blazored.Toast.Services;
using HNOne.Common;
using HNOne.Model;
using HNOne.Web.Commons;
using Newtonsoft.Json;

namespace HNOne.Web.Services
{
    public interface IApprovalService
    {
        Task<bool> UpdateApprovalAsync(string processKey, int userId, string token, string json, string approvalType = "");
    }
    public class ApprovalService : ApiServiceBase, IApprovalService
    {
        private IToastService _toastService { get; init; }
        public ApprovalService(IHttpClientFactory factory, ILogger<ApprovalService> logger, IToastService toastService)
            : base(factory, logger)
        {
            _toastService = toastService;
        }

        /// <summary>
        /// cập nhật thông tin phê duyệt
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="processKey"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public async Task<bool> UpdateApprovalAsync(string processKey, int userId, string token, string json, string approvalType = "")
        {
            try
            {
                RequestModel request = new RequestModel();
                request.process = processKey;
                request.type = approvalType;
                request.userId = userId;
                request.token = token;
                request.json = json;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_APPROVAL_DATA, request);
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
