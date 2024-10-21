using Microsoft.AspNetCore.Components;
using HNOne.Model;
using Blazored.Toast.Services;
using DevExpress.Blazor.Internal.Editors.Models;
using HNOne.Common;
using Blazored.LocalStorage;

namespace HNOne.Web.Controllers
{
    public class LoginController : ComponentBase
    {
        [Inject] NavigationManager _nav { get; set; }
        [Inject] IToastService _toastService { get; set; }
        [Inject] ILogger<LoginController> _logger { get; set; }
        [Inject] IEncryptHelper _encryptHelper { get; set; }
        [Inject] ILocalStorageService _localStorage { get; set; }

        #region Properties
        public string? ErrorMessage { get; set; }
        public bool IsShowLoading { get; set; } = false;
        public LoginRequestModel LoginRequest { get; set; } = new LoginRequestModel();
        public List<ComboboxModel>? ListBranch { get; set; }
        public bool IsShowPassword { get; set; } = false;
        #endregion

        private async Task getCompany()
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await _localStorage.ClearAsync(); // xóa hết dữ liệu lưu ở local store
                    await getCompany();
                    await InvokeAsync(StateHasChanged);
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "OnAfterRenderAsync");
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
