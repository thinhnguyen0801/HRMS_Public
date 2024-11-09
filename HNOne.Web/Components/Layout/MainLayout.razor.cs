using Blazored.Toast.Services;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using HNOne.Web.Commons;
using Newtonsoft.Json;
using HNOne.Common;
using Blazored.LocalStorage;

namespace HNOne.Web.Components.Layout
{
    public partial class MainLayout
    {
        [Inject] IJSRuntime _jsRuntime { get; init; }
        [Inject] AuthenticationStateProvider _authenticationStateProvider { get; init; }
        [Inject] IToastService toastService { get; init; }
        [Inject] ILogger<MainLayout> _logger { get; init; }
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IEncryptHelper  _encryptHelper { get; init; }
        [Inject] ILocalStorageService _localStorage { get; init; }

        #region Properties
        public List<BreadcrumbModel>? ListBreadcrumbs { get; set; }
        public List<MenuModel> ListMenus { get; set; } = new List<MenuModel>();
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int BrandId { get; set; } = 0;
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
                var userLogin = await ((ApiAuthenticationStateProvider)_authenticationStateProvider!).GetAuthenticationStateAsync();
                if (userLogin != null)
                {
                    string claimKey = userLogin.User.Claims.FirstOrDefault(m => m.Type == "JSON_USER")?.Value + "";
                    ResUserModel? result = JsonConvert.DeserializeObject<ResUserModel>(_encryptHelper.Decrypt(claimKey));
                    if (result == null) return; // laod lần đầu
                    UserId = result.userId;
                    Token = $"{result.token}";
                    FullName = $"{result.employeeName}";
                    UserCode = $"{result.branchCode} - {result.employeeCode}";
                    BrandId = result.branchId;
                    await getMenus();
                    // kiểm tra cái key bgcolor default -> thì set lại color theo user
                    if (await _localStorage.ContainKeyAsync("bgcolor"))
                    {
                        string? color = await _localStorage.GetItemAsStringAsync("bgcolor");
                        await _jsRuntime.InvokeVoidAsync("setActiveStyle", color);
                    }
                }    
                    
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
            try
            {
                RequestModel request = new RequestModel();
                request.token = Token;
                request.userId = UserId;
                ListMenus = await _masterDataService.GetMenuAsync(request) ?? [];
                var lstPermission = ListMenus.Where(m => !string.IsNullOrEmpty(m.link) && m.link != "#").Select(m=> $"{m.link}").ToList();
                if(!lstPermission.IsNullOrEmpty())
                {
                    // lưu vào local store nhưng menu nào bạn được phép truy cập
                    var checkExists = await _localStorage.ContainKeyAsync("authMenu");
                    if (checkExists) await _localStorage.RemoveItemAsync("authMenu");
                    await _localStorage.SetItemAsync<string>("authMenu", _encryptHelper.Encrypt(JsonConvert.SerializeObject(lstPermission)));
                }    
            }
            catch (Exception) { throw; }
        }
        #endregion
    }
}
