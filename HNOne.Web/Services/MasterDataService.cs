using Blazored.Toast.Services;
using DevExpress.ClipboardSource.SpreadsheetML;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http.Headers;

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
        public async Task<List<MenuModel>?> GetMenuAsync(RequestModel request)
        {
            
            try
            {
                List<MenuModel>? data = null;
                request.process = ProcessConstants.GET_MENU;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<MenuModel>>();
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
                return false;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// lấy danh sách phòng ban
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<DepartmentModel>?> GetDepartmentAsync(int userId, string token = "")
        {

            try
            {
                List<DepartmentModel>? data = null;
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_DEPARTMENT;
                request.userId = userId;
                request.token = token;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<DepartmentModel>>();
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
                _logger.LogError(ex, "GetDepartmentAsync");
                throw ex;
            }
        }

        /// <summary>
        /// cập nhật thông tin phòng ban
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="processKey"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public async Task<bool> UpdateDepartmentAsync(string processKey, int userId, string token, string json)
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
                return false;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// lấy danh sách chức danh
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<TitleModel>?> GetTitleAsync(int userId, string token = "")
        {

            try
            {
                List<TitleModel>? data = null;
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_TITLE;
                request.userId = userId;
                request.token = token;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<TitleModel>>();
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
                _logger.LogError(ex, "GetTitleAsync");
                throw ex;
            }
        }

        /// <summary>
        /// cập nhật thông tin chức danh
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="processKey"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public async Task<bool> UpdateTitleAsync(string processKey, int userId, string token, string json)
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
                return false;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// lấy danh sách chức vụ
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<PositionModel>?> GetPositionAsync(int userId, string token = "")
        {

            try
            {
                List<PositionModel>? data = null;
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_POSITION;
                request.userId = userId;
                request.token = token;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<PositionModel>>();
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
                _logger.LogError(ex, "GetPositionAsync");
                throw ex;
            }
        }

        /// <summary>
        /// cập nhật thông tin chức vụ
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="processKey"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePositionAsync(string processKey, int userId, string token, string json)
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
                return false;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// lấy danh sách loại hợp đồng
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<ContractTypeModel>?> GetContractTypeAsync(int userId, string token = "")
        {

            try
            {
                List<ContractTypeModel>? data = null;
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_CONTRACTTYPE;
                request.userId = userId;
                request.token = token;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<ContractTypeModel>>();
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
                _logger.LogError(ex, "GetContractTypeAsync");
                throw ex;
            }
        }

        /// <summary>
        /// cập nhật thông tin loại hợp đồng
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="processKey"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public async Task<bool> UpdateContractTypeAsync(string processKey, int userId, string token, string json)
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
                return false;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// lấy danh sách danh mục lý do
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<ReasonCategorieModel>?> GetReasonCategorieAsync(int userId, string token = "")
        {

            try
            {
                List<ReasonCategorieModel>? data = null;
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_REASONCATEGORIE;
                request.userId = userId;
                request.token = token;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<ReasonCategorieModel>>();
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
                _logger.LogError(ex, "GetReasonCategorieAsync");
                throw ex;
            }
        }

        /// <summary>
        /// cập nhật thông tin danh mục lý do
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="processKey"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public async Task<bool> UpdateReasonCategorieAsync(string processKey, int userId, string token, string json)
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
                return false;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// lấy danh sách enum
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="enumType"></param>
        /// <param name="isShowToast"></param>
        /// <returns></returns>
        public async Task<List<EnumCatagoryModel>?> GetEnumAsync(int userId, string token, string? enumType, bool isShowToast = false)
        {

            try
            {
                List<EnumCatagoryModel>? data = null;
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_ENUM;
                request.userId = userId;
                request.token = token;
                request.opt = enumType;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<EnumCatagoryModel>>();
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
                _logger.LogError(ex, "GetEnumAsync");
                throw;
            }
        }

        /// <summary>
        /// lấy danh sách loại lương
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="isShowToast"></param>
        /// <returns></returns>
        public async Task<List<SalaryCategoryModel>?> GetSalaryCatagoryAsync(int userId, string token, string condition = "", bool isShowToast = false)
        {

            try
            {
                List<SalaryCategoryModel>? data = null;
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_SALARY_CATEGORY;
                request.userId = userId;
                request.token = token;
                request.opt = condition;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<SalaryCategoryModel>>();
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
                _logger.LogError(ex, "GetSalaryCatagoryAsync");
                throw ex;
            }
        }

        /// <summary>
        /// cập nhật thông tin loại lương
        /// </summary>
        /// <param name="processKey"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public async Task<bool> UpdateSalaryCategoryAsync(string processKey, int userId, string token, string json)
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
        /// lấy danh sách cấu hình tính lương
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="isShowToast"></param>
        /// <returns></returns>
        public async Task<List<SalaryConfigurationModel>?> GetSalaryConfigAsync(int userId, string token, bool isShowToast = false)
        {

            try
            {
                List<SalaryConfigurationModel>? data = null;
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_SALARY_CONFIG;
                request.userId = userId;
                request.token = token;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<SalaryConfigurationModel>>();
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
                _logger.LogError(ex, "GetSalaryCatagoryAsync");
                throw;
            }
        }


        /// <summary>
        /// cập nhật thông tin cấu hình tính lương
        /// </summary>
        /// <param name="processKey"></param>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public async Task<bool> UpdateSalaryConfigAsync(string processKey, int userId, string token, string json)
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
        /// đánh số chứng từ
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="branchId"></param>
        /// <param name="type"></param>
        /// <param name="opt"></param>
        /// <param name="opt1"></param>
        /// <param name="opt2"></param>
        /// <returns></returns>
        public async Task<string?> GetDocumentNo(int userId, string token, int branchId, string? type, string? opt = "", string? opt1 = "", string? opt2 = "")
        {
            try
            {
                string voucherNo = string.Empty;
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_DOCUMENT_NO;
                request.userId = userId;
                request.token = token;
                request.branchId = branchId;
                request.type = type;
                request.opt = opt;
                request.opt1 = opt1;
                request.opt2 = opt2;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResponseModel>();
                    if (response == null || response.status != StatusCodes.Status200OK 
                        || string.IsNullOrEmpty($"{response.data}"))
                    {
                        _toastService.ShowInfo(MessageConstants.MESSAGE_DOCUMENT_NO_EMPTY);
                        return "";
                    }
                    voucherNo = $"{response.data}";
                }
                return voucherNo;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Lấy danh sách Quốc gia, Tỉnh thành, Quận/Huyện, Xã/Phường
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="branchId"></param>
        /// <param name="type"></param>
        /// <param name="opt"></param>
        /// <param name="opt1"></param>
        /// <param name="opt2"></param>
        /// <returns></returns>
        public async Task<List<ComboboxModel>?> GetLocationAsync(int userId, string token, string? type, string? opt = "", string? opt1 = "", string? opt2 = "")
        {
            try
            {
                List<ComboboxModel>? data = null;
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_LOCATION;
                request.userId = userId;
                request.token = token;
                request.type = type;
                request.opt = opt;
                request.opt1 = opt1;
                request.opt2 = opt2;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (!checkContent) _toastService.ShowInfo(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<ComboboxModel>>();
                    if (response == null || response.status != StatusCodes.Status200OK)
                    {
                        return data;
                    }
                    data = response.data?.ToList();
                }
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetLocationAsync");
                throw;
            }
        }

        /// <summary>
        /// lấy danh sách com dưới sql server
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<T>?> GetMasterDataAsync<T>(RequestModel request, bool isShowToast = false) where T : class
        {

            try
            {
                List<T>? data = null;
                request.process = ProcessConstants.GET_COMBO_MASTER_DATA;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA, request);
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
        /// upload hình ảnh lên server
        /// </summary>
        /// <param name="listImages"></param>
        /// <param name="subFolder"></param>
        /// <returns></returns>
        public async Task<List<FileUploadModel>?> UploadImagesAsync(List<FileUploadModel> listImages, string subFolder)
        {
            bool isSuccess = false;
            try
            {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using var content = new MultipartFormDataContent();
                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
                if (listImages != null && listImages.Any())
                {
                    var lstUpload = listImages.Where(m => m.isDelete == false); // lấy ra hình ảnh nào cần upload
                    foreach (var file in lstUpload)
                    {
                        if (File.Exists($"{file.filePath}"))
                        {
                            content.Add(new StreamContent(File.OpenRead(@$"{file.filePath}")), name: "\"files\"", fileName: file.fileName + "");
                        }
                    }
                }
                HttpResponseMessage httpResponse = await _httpClient.PostAsync($"api/{EnpointConstants.MASTERDATA_UPLOAD_IMAGE}?subFolder={subFolder}", content);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _toastService.ShowInfo(MessageConstants.MESSAGE_LOGIN_EXPIRED);
                    return default;
                }
                if (!checkContent) _toastService.ShowError(MessageConstants.MESSAGE_JSON_INVALID);
                else
                {
                    var resContent = await httpResponse.Content.ReadAsStringAsync();
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        isSuccess = true;
                        return JsonConvert.DeserializeObject<List<FileUploadModel>>(resContent);
                    }    
                    var oMessage = JsonConvert.DeserializeObject<ResponseModel>(resContent);
                    _toastService.ShowInfo($"{oMessage?.message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadImagesAsync");
                _toastService.ShowError(ex.Message);
            }
            finally
            {
                // xóa hình ảnh trong folder tạm
                if (isSuccess && listImages != null && listImages.Any())
                {
                    foreach (var file in listImages)
                    {
                        if (File.Exists($"{file.filePath}")) File.Delete($"{file.filePath}");
                    }
                }
            }
            return default;

        }
    }
}
