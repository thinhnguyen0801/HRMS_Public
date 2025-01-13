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
using Microsoft.AspNetCore.SignalR.Client;
using HNOne.Model.Entities;

namespace HNOne.Web.Components.Layout
{
    public partial class MainLayout //: IAsyncDisposable
    {
        [Inject] IJSRuntime _jsRuntime { get; init; }
        [Inject] AuthenticationStateProvider _authenticationStateProvider { get; init; }
        [Inject] IToastService toastService { get; init; }
        [Inject] ILogger<MainLayout> _logger { get; init; }
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IEncryptHelper  _encryptHelper { get; init; }
        [Inject] ILocalStorageService _localStorage { get; init; }
        [Inject] IConfiguration _configuration { get; init; }
        HubConnection? _hubConnection;
        #region Properties
        public List<BreadcrumbModel>? ListBreadcrumbs { get; set; }
        public List<MenuModel> ListMenus { get; set; } = new List<MenuModel>();
        public int UserId { get; set; }
        public int EmployeeId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int BranchId { get; set; } = 0;

        public List<NotificationModel> ListNotification = new List<NotificationModel>();
        public int TotalNotifiCations { get; set; } = 0; // tổng số thông báo
        public bool IsReceiveNotification { get; set; } = true; // là nhận thông báo
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
                    BranchId = result.branchId;
                    EmployeeId = result.employeeId;
                    await getMenus();
                    _= getNotifications();
                    // kiểm tra cái key bgcolor default -> thì set lại color theo user
                    if (await _localStorage.ContainKeyAsync("bgcolor"))
                    {
                        string? color = await _localStorage.GetItemAsStringAsync("bgcolor");
                        await _jsRuntime.InvokeVoidAsync("setActiveStyle", color);
                    }

                    // kết nối tới hub
                    //string apiUrl = _configuration.GetSection("appSettings:ApiUrl").Value + "";
                    //_hubConnection = new HubConnectionBuilder().WithUrl($"{apiUrl}notificationHub?userId={result.employeeId}").Build();
                    //_hubConnection.On<string>("ReceiveMessage", async (incomingMessage) =>
                    //{
                    //    if(IsReceiveNotification) toastService.ShowInfo(incomingMessage);
                    //    await getNotifications();
                    //});
                    //await _hubConnection.StartAsync();
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
                request.type = ProcessConstants.GET_MENU_TYPE_MENU;
                ListMenus = await _masterDataService.GetMenuAsync(request) ?? [];
                var lstPermission = ListMenus.Where(m => !string.IsNullOrEmpty(m.link) && m.link != "#")
                        .Select(m=> new { link = $"{m.link}", menuId = $"{m.menuID}"} ).ToList();
                if(!lstPermission.IsNullOrEmpty())
                {
                    // lưu vào local store nhưng menu nào bạn được phép truy cập
                    //var checkExists = await _localStorage.ContainKeyAsync("authMenu");
                    //if (checkExists) await _localStorage.RemoveItemAsync("authMenu");
                    await _localStorage.SetItemAsync<string>("authMenu", _encryptHelper.Encrypt(JsonConvert.SerializeObject(lstPermission)));
                }    
            }
            catch (Exception) { throw; }
        }
        
        /// <summary>
        /// lấy danh sách thông báo của nhân viên
        /// </summary>
        /// <returns></returns>
        private async Task getNotifications()
        {
            try
            {
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.token = Token;
                request.branchId = BranchId;
                request.type = ProcessConstants.GET_COMBO_TYPE_NOTIFICATION_BY_EMPLOYEE_MAIN;
                request.opt = EmployeeId.ToString();
                var result = await _masterDataService.GetMasterDataAsync<NotificationModel>(request);
                if (!result.IsNullOrEmpty())
                {
                    TotalNotifiCations = result![0].totalRow;
                    ListNotification = result!;
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch { }
             
        }

        //public async ValueTask DisposeAsync()
        //{
        //    if (_hubConnection != null)
        //    {
        //        await _hubConnection.StopAsync();
        //        await _hubConnection.DisposeAsync();
        //    }
        //}
        #endregion
    }
}
