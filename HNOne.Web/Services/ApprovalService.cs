using Blazored.Toast.Services;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using Newtonsoft.Json;

namespace HNOne.Web.Services
{
    public interface IApprovalService
    {
        Task<List<ApprovalModel>?> GetApprovalAsync(int userId, int branchId, int employeeId
            , string token, string approvalType = "O", DateTime? fromDate = null, DateTime? toDate = null);
        Task<bool> UpdateApprovalAsync(string processKey, int userId, string token, string json, string approvalType = "");
        Task<string?> GetFunDocumentHistoryAsync(int userId, int branchId, string token, string objType, int documentId, string opt = "", string opt1 = "");
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
        /// lấy danh sách chứng từ phê duyệt
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="branchId"></param>
        /// <param name="employeeId"></param>
        /// <param name="token"></param>
        /// <param name="approvalType"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public async Task<List<ApprovalModel>?> GetApprovalAsync(int userId, int branchId, int employeeId
            , string token, string approvalType = "O", DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                List<ApprovalModel>? data = null;
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_APPROVAL;
                request.userId = userId;
                request.branchId = branchId;
                request.employeeId = employeeId;
                request.token = token;
                request.type = approvalType;
                request.fromDate = fromDate;
                request.toDate = toDate;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_APPROVAL_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<ApprovalModel>>();
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

        /// <summary>
        /// lấy danh sách lịch sử chứng từ
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="branchId"></param>
        /// <param name="token"></param>
        /// <param name="objType"></param>
        /// <param name="employeeId"></param>
        /// <param name="opt"></param>
        /// <param name="opt1"></param>
        /// <returns></returns>
        public async Task<string?> GetFunDocumentHistoryAsync(int userId, int branchId, string token, string objType, int documentId, string opt = "", string opt1 = "")
        {
            try
            {
                string? strHistory = string.Empty;
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_DOCUMENT_HISTORY;
                request.userId = userId;
                request.branchId = branchId;
                request.token = token;
                request.type = objType;
                request.documentId = documentId;
                request.opt = opt;
                request.opt1 = opt1;

                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_APPROVAL_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResponseModel>();
                    if (response == null || response.status != StatusCodes.Status200OK)
                    {
                        _toastService.ShowWarning(response?.message ?? MessageConstants.MESSAGE_IT_SUPPORT);
                        return strHistory;
                    }
                    strHistory = response.data?.ToString();
                }
                return strHistory;
            }
            catch (Exception) { throw; }
        }
    }
}
