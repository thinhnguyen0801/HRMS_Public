using Blazored.Toast.Services;
using DevExpress.Pdf.Native.BouncyCastle.Asn1.Ocsp;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
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
                return true;
            }
            catch (Exception) { throw; }
        }
    }
}
