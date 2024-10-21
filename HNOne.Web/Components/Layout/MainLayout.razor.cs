using Blazored.Toast.Services;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace HNOne.Web.Components.Layout
{
    public partial class MainLayout
    {
        [Inject] HttpClient Http { get; init; }
        [Inject] NavigationManager _navManager { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; init; }
        [Inject] AuthenticationStateProvider _authenticationStateProvider { get; init; }
        [Inject] IToastService toastService { get; init; }
        [Inject] ILogger<MainLayout> _logger { get; init; }
        [Inject] IMasterDataService _masterDataService { get; init; }

        #region Properties
        public List<BreadcrumbModel>? ListBreadcrumbs { get; set; }
        public List<Menus> ListMenus { get; set; } = new List<Menus>();
        #endregion

        EventCallback<List<BreadcrumbModel>> BreadcrumbsHandler =>
        EventCallback.Factory.Create(this, (Action<List<BreadcrumbModel>>)NotifyBreadcrumb);
        private void NotifyBreadcrumb(List<BreadcrumbModel> _breadcrumbs)
        {
            try
            {
                ListBreadcrumbs = _breadcrumbs;
                StateHasChanged();
            }
            catch (Exception) { }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            try
            {
                //ListMenus = await Http!.GetFromJsonAsync<List<Menus>>("https://localhost:7140/menus.json") ?? new List<Menus>();
                await getMenus();
            }
            catch (Exception) { }
        }

        /// <summary>
        /// đóng hết menu nào đang mở khi điều hướng page menu
        /// </summary>
        protected async void CloseMenuHandler()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("closeMenuHandler");
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CloseMenuHandler");
            }
        }

        #region Private Function
        private async Task getMenus()
        {
            RequestModel request = new RequestModel();
            request.token = "";
            ListMenus = await _masterDataService.GetMenuAsync(request) ?? []; 
        }
        #endregion
    }
}
