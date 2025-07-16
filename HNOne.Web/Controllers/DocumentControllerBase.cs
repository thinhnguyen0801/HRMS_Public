using Blazored.LocalStorage;
using Blazored.Toast.Services;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Models;
using HNOne.Web.Services;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    /// <summary>
    /// hainguyen create 2024.10.22
    /// Controller dùng chung
    /// </summary>
    public class DocumentControllerBase : ComponentBase
    {
        [Inject] protected ILogger<DocumentControllerBase> _logger { get; init; }
        [Inject] protected ILoadingService _loadingService { get; init; }
        [Inject] protected NavigationManager _navigationManager { get; init; }
        [Inject] protected IToastService _toastService { get; init; }
        [Inject] private AuthenticationStateProvider _authenticationStateProvider { get; init; }
        [Inject] protected ILocalStorageService _localStorageService { get; init; }
        [Inject] protected IProgressService _progressService { get; init; }
        [Inject] protected IEncryptHelper _encryptHelper { get; init; }
        [Inject] private IPermissionService _permissionService { get; init; }

        #region Properties
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public int BranchId { get; set; }
        public string? BranchIds { get; set; }
        public int EmployeeId { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentIds { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string MenuId { get; set; } = string.Empty; // menu được chọn

        [CascadingParameter]
        public EventCallback<List<BreadcrumbModel>> NotifyBreadcrumb { get; set; }
        public List<BreadcrumbModel>? ListBreadcrumbs { get; set; }
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender)
            {
                try
                {
                    var userLogin = await ((ApiAuthenticationStateProvider)_authenticationStateProvider!).GetAuthenticationStateAsync();
                    if (userLogin != null)
                    {

                        string claimKey = userLogin.User.Claims.FirstOrDefault(m => m.Type == "JSON_USER")?.Value + "";
                        ResUserModel? result = JsonConvert.DeserializeObject<ResUserModel>(_encryptHelper.Decrypt(claimKey));
                        if (result == null) return; // laod lần đầu
                        UserId = result.userId;
                        Token = $"{result.token}";
                        IsAdmin = result.isAdmin;
                        BranchId = result.branchId;
                        EmployeeId = result.employeeId;
                        EmployeeCode = result.employeeCode;
                        EmployeeName = result.employeeName;
                        DepartmentId = result.departmentId;
                        BranchIds = result.branchIds;
                        DepartmentIds = result.departmentIds;
                    }
                }
                catch (Exception ex) { _logger.LogError(ex, "OnAfterRenderAsync"); }
            }    
        }

        #region Protected Functions
        protected async Task ShowLoading(bool isShow = true)
        {
            if (isShow)
            {
                _loadingService.ShowLoading(isShow);
                await Task.Yield();
                return;
            }
            _loadingService.ShowLoading(isShow);
        }

        protected void ShowSuccess(string message) => _toastService.ShowSuccess(message);
        protected void ShowWarning(string message) => _toastService.ShowWarning(message);
        protected void ShowError(string message) => _toastService.ShowError(message);
        protected void ShowInfo(string message) => _toastService.ShowInfo(message);
        
        /// <summary>
        /// kiểm tra quyền truy cập menu
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        protected async Task<string> CheckMenuPermissionAsync(string link)
        {
            try
            {
                await Task.Delay(10);
                var checkSession = await _localStorageService.ContainKeyAsync("authMenu");
                if(!checkSession)
                {
                    _navigationManager.NavigateTo("/401");
                    return "401";
                }
                string? planText = await _localStorageService.GetItemAsync<string>("authMenu");
                var lstItem = JsonConvert.DeserializeObject<List<MenuModel>>(_encryptHelper.Decrypt(planText));
                var checkMenu = lstItem?.FirstOrDefault(m => m.link == link || m.link == $"/{link}");
                if(checkMenu == null)
                {
                    _navigationManager.NavigateTo("/401");
                    return "401";
                }
                MenuId = $"{checkMenu.menuID}";
                return $"{checkMenu.menuID}";
            }
            catch(Exception){ throw; }
        }

        /// <summary>
        /// Lấy ra thông tin menuid
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        protected async Task<string> GetMenuId(string link)
        {
            try
            {
                await Task.Delay(10);
                var checkSession = await _localStorageService.ContainKeyAsync("authMenu");
                if (!checkSession) return "";
                string? planText = await _localStorageService.GetItemAsync<string>("authMenu");
                var lstItem = JsonConvert.DeserializeObject<List<MenuModel>>(_encryptHelper.Decrypt(planText));
                var checkMenu = lstItem?.FirstOrDefault(m => m.link == link || m.link == $"/{link}");
                return $"{checkMenu?.menuID}";
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// lấy ra danh sách key để kiểm tra quyền
        /// </summary>
        /// <param name="menuId"></param>
        /// <returns></returns>
        protected async Task<List<string>> CheckEventPermission(string menuId)
            => await _permissionService.GetEventKeyAsync(UserId, Token, menuId);
        #endregion

    }
}
