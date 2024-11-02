using Microsoft.AspNetCore.Components;
using HNOne.Model;
using Blazored.Toast.Services;
using HNOne.Web.Services.Interfaces;
using HNOne.Common;
using Blazored.LocalStorage;
using HNOne.Web.Services;
using Microsoft.AspNetCore.WebUtilities;

namespace HNOne.Web.Controllers
{
    public class LoginController : ComponentBase
    {
        [Inject] NavigationManager _nav { get; set; }
        [Inject] IToastService _toastService { get; set; }
        [Inject] ILogger<LoginController> _logger { get; set; }
        [Inject] IEncryptHelper _encryptHelper { get; set; }
        [Inject] ILocalStorageService _localStorage { get; set; }
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IProgressService _progressService { get; init; }
        [Inject] IUserService _userService { get; init; }

        #region Properties
        public bool IsShowLoading { get; set; } = false;
        public LoginRequestModel LoginRequest { get; set; } = new LoginRequestModel();
        public List<ComboboxModel>? ListBranch { get; set; }
        public bool IsShowPassword { get; set; } = false;
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await _progressService.SetPercent(0.4);
                    await _localStorage.ClearAsync(); // xóa hết dữ liệu lưu ở local store
                    var lstBranch = await _masterDataService.GetBranchAsync(1, "");
                    await _progressService.SetPercent(0.6);
                    ListBranch = lstBranch?.Select(m => new ComboboxModel() { id = m.branchId, code = m.branchId.ToString(), name = m.branchName })?.ToList();
                    LoginRequest.branchId = ListBranch?.FirstOrDefault()?.code;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OnAfterRenderAsync");
                    _toastService.ShowWarning(ex.Message);
                }
                finally
                {
                    await _progressService.Done();
                    await InvokeAsync(StateHasChanged);
                }
            }
        }


        #region Protected Functions
        protected async Task LoginHandler()
        {
            try
            {
                if (IsShowLoading) return;
                IsShowLoading = true;
                await Task.Delay(75);
                string userName = _encryptHelper.Encrypt(LoginRequest.userName);
                string password = _encryptHelper.Encrypt(LoginRequest.password);
                LoginRequestModel request = new LoginRequestModel();
                request.userName = userName;
                request.password = password;
                request.branchId = LoginRequest.branchId;
                request.rememberMe = LoginRequest.rememberMe;
                var errorMessage = await _userService.LoginAsync(request);
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    _toastService.ShowInfo(errorMessage);
                    return;
                }
                var uri = _nav.ToAbsoluteUri(_nav.Uri);
                string key = "/trang-chu";
                if (uri != null && $"{uri}".Contains("returnUrl"))
                {
                    var queryStrings = QueryHelpers.ParseQuery(uri.Query);
                    if (queryStrings.Count() > 0
                        && queryStrings.TryGetValue("returnUrl", out var returnUrl)
                        && !$"{returnUrl}".Contains("dang-xuat"))
                    {
                        key = returnUrl.ToString();
                    }
                }    
                await Task.Delay(100);
                _nav.NavigateTo($"{key}", true);
            }
            catch (Exception ex)
            {
                _toastService.ShowError($"{ex.Message}");
                _logger.LogError(ex, "LoginHandler");
            }
            finally
            {
                IsShowLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion
    }
}
