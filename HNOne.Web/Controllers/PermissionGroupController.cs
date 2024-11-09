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
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "tên tài khoản");
                fieldName = "txtName";
                return;
            }
        }

        /// <summary>
        /// kiểm tra dữ liệu trước khi lưu phân quyền
        /// </summary>
        /// <param name="errorMessage"></param>
        private void validateForSavePermission(ref string errorMessage)
        {
            if (SelectedItems.IsNullOrEmpty())
            {
                errorMessage = MessageConstants.MESSAGE_NO_CHOSE_DATA;
                return;
            }
            if (SelectedItems!.Count > 1)
            {
                errorMessage = MessageConstants.MESSAGE_ONLY_ONE_SELECTION_ALLOWED;
                return;
            }
            if(ListMenuAuth.IsNullOrEmpty())
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Phân quyền");
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

        protected async Task DeleteDataHandler()
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
        
        /// <summary>
        /// cập nhật quyền cho nhóm
        /// </summary>
        /// <returns></returns>
        protected async Task SavePermissionHandler()
        {
            try
            {
                string errorMessage = string.Empty;
                validateForSavePermission(ref errorMessage);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    return;
                }
                var permision = JsonConvert.DeserializeObject<PermissionGroupModel>(JsonConvert.SerializeObject(SelectedItems![0]));
                errorMessage = string.Format(MessageConstants.MESSAGE_CONFIRM_UPDATE_FORMAT, $"quyền cho nhóm {permision!.code}");
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                List<object> lstEvent = new List<object>();
                foreach(var menu in ListMenuAuth!)
                {
                    if(!menu.listEvent.IsNullOrEmpty())
                    {
                        var dataEvetn = menu.listEvent!.Select(m => new
                        {
                            m.eventId,
                            m.actionName,
                            m.isAllow,
                            userSign = UserId,
                            userSign2 = UserId,
                        });
                        lstEvent.AddRange(dataEvetn);
                    }    
                }    
                DataUpdate.userSign = UserId;
                DataUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(lstEvent);
                isConfirm = await _userDataService.UpdatePerGroupControlAsync(permision!.id, UserId, Token, content);
                if(isConfirm)
                {
                    // load lại dữ liệu cấu hình
                    await ItemGroupChangedHandler(SelectedItems![0]);
                }    
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "SavePermissionHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(50);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        
        /// <summary>
        /// call lấy dữ liệu phân quyền
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        protected async Task ItemGroupChangedHandler(object selected)
        {
            try
            {
                if(ListMenuAuth.IsNullOrEmpty())
                {
                    ShowWarning(string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Phân quyền"));
                    return;
                }
                await ShowLoading();
                ListMenuAuth!.Update(m => m.listEvent?.Update(m => m.isAllow = false));
                PermissionGroupModel permissionSelected = (PermissionGroupModel)selected;
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.token = Token;
                request.branchId = BranchId;
                request.documentId = permissionSelected.id;
                request.process = ProcessConstants.GET_PER_GROUP_ACCESS_CONTROL;
                var listResult = await _userDataService.GetMasterDataAsync<EventConfigModel>(request);
                if (!listResult.IsNullOrEmpty())
                {
                    foreach (var menu in ListMenuAuth!)
                    {
                        if (menu.listEvent.IsNullOrEmpty()) continue;
                        foreach (var item in menu.listEvent!)
                        {
                            foreach (var permiss in listResult!)
                            {
                                if (item.eventId == permiss.eventId)
                                {
                                    item.isAllow = permiss.isAllow;
                                    break;
                                }
                            }
                        }
                    }
                }
                GridMenuAuth?.Reload();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "SavePermissionHandler");
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
