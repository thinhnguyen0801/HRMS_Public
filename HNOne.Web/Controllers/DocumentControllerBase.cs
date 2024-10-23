using Blazored.LocalStorage;
using Blazored.Toast.Services;
using HNOne.Web.Commons;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Reflection;

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

        #region Properties
        public int BranchId { get; set; }
        public string BranchCode { get; set; } = "";
        public int UserId { get; set; }
        public string IsAdmin { get; set; } = "N";
        public string UserCode { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

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
        #endregion

    }
}
