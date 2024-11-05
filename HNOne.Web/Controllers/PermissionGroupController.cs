using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Reflection;

namespace HNOne.Web.Controllers
{
    public class PermissionGroupController : DocumentControllerBase
    {
        [Inject] IUserService _userDataService { get; init; }
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        const string MENU_TYPE = "AUTHENTICATION";

        #region Properties
        public List<PermissionGroupModel>? ListData { get; set; }
        public IGrid? GridData { get; set; }
        public IReadOnlyList<object>? SelectedItems { get; set; } = null;
        public List<MenuModel>? ListMenuAuth { get; set; } // danh sách menu để phân quyền 
        public IGrid? GridMenuAuth { get; set; }
        
        public PermissionGroupModel DataUpdate { get; set; } = new PermissionGroupModel();

        public bool IsCreate { get; set; } = true;
        public bool IsShowDialog { get; set; }
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Hệ thống"),
                        new BreadcrumbModel("Nhóm quyền", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await getPermissionGroup();
                    await getMenuAuth();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OnAfterRenderAsync");
                    ShowError(ex.Message);
                }
                finally
                {
                    await ShowLoading(false);
                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        #region Private Functions
        private async Task getPermissionGroup()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.branchId = BranchId;
            request.opt = "";
            ListData = new List<PermissionGroupModel>();
            ListData = await _userDataService.GetPermissionGroup(request);
        }

        /// <summary>
        /// lấy danh sách phân quyền
        /// </summary>
        /// <returns></returns>
        private async Task getMenuAuth()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.token = Token;
            request.branchId = BranchId;
            request.type = MENU_TYPE;
            ListMenuAuth = new List<MenuModel>();
            ListMenuAuth = await _masterDataService.GetMenuAsync(request);
        }

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(DataUpdate.name))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "tên tài khoản");
                fieldName = "txtName";
                return;
            }
        }

        #endregion

        #region Protected Functions
        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getPermissionGroup();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ReLoadDataHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(50);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, PermissionGroupModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    DataUpdate = new PermissionGroupModel();
                }
                else
                {
                    DataUpdate.id = pItemDetails!.id;
                    DataUpdate.code = pItemDetails!.code;
                    DataUpdate.name = pItemDetails!.name;
                    DataUpdate.remark = pItemDetails!.remark;
                    DataUpdate.isActive = pItemDetails!.isActive;
                    IsCreate = false;
                }
                IsShowDialog = true;
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "OnOpenDialogHandler");
                ShowError(ex.Message);
            }
        }

        protected async Task SaveDataHandler()
        {
            try
            {
                string errorMessage = string.Empty;
                string fieldName = string.Empty;
                validateForSave(ref errorMessage, ref fieldName);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                    return;
                }
                errorMessage = IsCreate ? MessageConstants.MESSAGE_CONFIRM_ADD : MessageConstants.MESSAGE_CONFIRM_UPDATE;
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = IsCreate ? ProcessConstants.POST_PER_GROUP : ProcessConstants.PUT_PER_GROUP;
                DataUpdate.userSign = UserId;
                DataUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(DataUpdate);
                isConfirm = await _userDataService.UpdatePermissionGroupAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getPermissionGroup();
                    IsShowDialog = false;
                    SelectedItems = null;
                }
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "SaveDataHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(50);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        public async Task DeleteDataHandler()
        { 
            try
            {

            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "DeleteDataHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(50);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion
    }
}
