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
using System.Threading.Tasks;

namespace HNOne.Web.Controllers
{
    public class PermissionGroupController : DocumentControllerBase
    {
        [Inject] IUserService _userDataService { get; init; }
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        const string MENU_TYPE = "AUTHENTICATION";
        const string MENU_TYPE_DATA = "AUTHENTICATION_DATA";
        const string STRING_KEY_EVENT_POST = "PERMISSION_GROUP_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "PERMISSION_GROUP_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "PERMISSION_GROUP_CONTROLLER_DELETE";
        #region Properties
        public int ActiveTabIndex { get; set; } = 0;
        public List<PermissionGroupModel>? ListData { get; set; }
        public IGrid? GridData { get; set; }
        public IReadOnlyList<object>? SelectedItems { get; set; } = null;
        public List<MenuModel>? ListMenuAuth { get; set; } // danh sách menu để phân quyền 
        public IGrid? GridMenuAuth { get; set; }

        public List<MenuModel>? ListDataAuthTemp { get; set; } // danh sách data để phân quyền 
        public List<MenuModel>? ListDataAuth { get; set; } // danh sách data để phân quyền 
        public IGrid? GridDataAuth { get; set; }

        public PermissionGroupModel DataUpdate { get; set; } = new PermissionGroupModel();
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public bool IsCreate { get; set; } = true;
        public bool IsShowDialog { get; set; }
        public bool IsCheckAllEvent { get; set; }
        public bool IsCheckAllData { get; set; }
        // nút quyền
        public bool IsAllowPost { get; set; }
        public bool IsAllowDelete { get; set; }
        public bool IsAllowPut { get; set; }
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    string errMessage = await CheckMenuPermissionAsync("nhom-quyen");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Hệ thống"),
                        new BreadcrumbModel("Nhóm quyền", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    var task1 = getPermissionGroup();
                    var task2 = getMenuAuth();
                    var task3 = getDataAuth();
                    var task4 = buildComboboxAsync();
                    await Task.WhenAll(task1, task2, task3, task4);
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
        private async Task buildComboboxAsync()
        {
            try
            {
                var getTask1 = _masterDataService.GetBranchAsync(UserId, Token, BranchId, $"{BranchIds}");
                await Task.WhenAll(
                    getTask1
                    );
                ListCboBranch = (await getTask1)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "BuildComboAsync");
            }
        }

        /// <summary>
        /// kiểm tra quyền nút
        /// </summary>
        /// <returns></returns>
        private async Task checkPermission(string menuId)
        {
            List<string> lstKey = await CheckEventPermission(menuId);
            IsAllowPost = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_POST) != null;
            IsAllowDelete = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_DELETE) != null;
            IsAllowPut = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_PUT) != null;
        }

        private async Task getPermissionGroup()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.branchId = BranchId;
            request.opt = "";
            request.branchIds = BranchIds;
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

        /// <summary>
        /// lấy danh sách phân quyền dữ liệu
        /// </summary>
        /// <returns></returns>
        private async Task getDataAuth()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.token = Token;
            request.branchId = BranchId;
            request.type = MENU_TYPE_DATA;
            ListDataAuth = new List<MenuModel>();
            ListDataAuth = await _masterDataService.GetMenuAsync(request);
            ListDataAuthTemp = ListDataAuth;
        }

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(DataUpdate.name))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Tên tài khoản");
                fieldName = "txtName";
                return;
            }
            if (DataUpdate.branchId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Chi nhánh");
                fieldName = "txtBranchId";
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
        protected async Task RefreshHandler(bool isUserGroup = true)
        {
            try
            {
                await ShowLoading();
                if(isUserGroup) await getPermissionGroup();
                else
                {
                    var task2 = getMenuAuth();
                    var task3 = getDataAuth();
                    await Task.WhenAll(task2, task3);
                    if (!SelectedItems.IsNullOrEmpty()) await ItemGroupChangedHandler(SelectedItems![0]);
                }

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
                    DataUpdate.branchId = pItemDetails!.branchId;
                    DataUpdate.branchCode = pItemDetails!.branchCode;
                    DataUpdate.branchName = pItemDetails!.branchName;
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
                await checkPermission(MenuId);
                if ((IsCreate && !IsAllowPost) || (!IsCreate && !IsAllowPut))
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
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

        /// <summary>
        /// xóa dữ liệu
        /// </summary>
        /// <returns></returns>
        protected async Task DeleteDataHandler()
        { 
            try
            {
                await checkPermission(MenuId);
                if (!IsAllowDelete)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
                if (SelectedItems.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                await ShowLoading();
                string tableName = _encryptHelper.Encrypt(nameof(EnumObjType.PermissionGroups));
                string pKey = _encryptHelper.Encrypt(nameof(PermissionGroupModel.id));
                string fKey = _encryptHelper.Encrypt(nameof(UserModel.perGroupId));
                string ids = string.Join(",", SelectedItems!.Cast<PermissionGroupModel>().Select(m => m.id));
                string reasonDelete = "";
                string strResult = await _masterDataService.DeleteDynnamicAsync(UserId, Token, BranchId, tableName, pKey, fKey, ids, reasonDelete);
                if (strResult == "-1") return;
                if (strResult == StatusCodes.Status200OK.ToString())
                {
                    await getPermissionGroup();
                    SelectedItems = null;
                    return;
                }
                await Task.Delay(75);
                await ShowLoading(false);
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_NOTIFICATION, $"{strResult} ", isShowFooter: false);
                return;
                
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
                await checkPermission(MenuId);
                if (!IsAllowPut)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
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
                var lstAutData = ListDataAuth!.Select(m => new
                {
                    type = m.parentID,
                    code = m.menuID,
                    m.isAllow,
                    userSign = UserId,
                    userSign2 = UserId,
                });
                DataUpdate.userSign = UserId;
                DataUpdate.userSign2 = UserId;
                string jsonAuthEvent = JsonConvert.SerializeObject(lstEvent);
                string jsonAuthData = JsonConvert.SerializeObject(lstAutData);
                isConfirm = await _userDataService.UpdatePerGroupControlAsync(permision!.id, UserId, Token, jsonAuthEvent, jsonAuthData);
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
                    ShowWarning(string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Phân quyền chức năng"));
                    return;
                }
                if (ListDataAuthTemp.IsNullOrEmpty())
                {
                    ShowWarning(string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Phân quyền dữ liệu"));
                    return;
                }
                PermissionGroupModel permissionSelected = (PermissionGroupModel)selected;
                if (permissionSelected == null) return;
                await ShowLoading();
                IsCheckAllEvent = false;
                ListMenuAuth!.Update(m => m.listEvent?.Update(m => m.isAllow = false));
                if (IsAdmin) ListDataAuth = ListDataAuthTemp; // nếu là admin thì được thấy hết
                else ListDataAuth = ListDataAuthTemp!.Where(m => m.branchId == permissionSelected.branchId).ToList();
                ListDataAuth!.Update(m => m.isAllow = false);
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.token = Token;
                request.branchId = BranchId;
                request.documentId = permissionSelected.id;
                request.process = ProcessConstants.GET_PER_GROUP_ACCESS_CONTROL;
                var task1 = _userDataService.GetMasterDataAsync<EventConfigModel>(request);
                request.process = ProcessConstants.GET_DATA_PER_GROUP;
                var task2 = _userDataService.GetMasterDataAsync<MenuModel>(request);
                await Task.WhenAll(task1, task2);
                var listResult = await task1;
                var listDataAuth = await task2;
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
                    if(ActiveTabIndex == 0) GridMenuAuth?.Reload();
                }
                if(!listDataAuth.IsNullOrEmpty())
                {
                    foreach (var item in ListDataAuth!)
                    {
                        foreach (var permiss in listDataAuth!)
                        {
                            if (item.menuID == permiss.menuID && item.parentID == permiss.parentID)
                            {
                                item.isAllow = permiss.isAllow;
                                break;
                            }
                        }
                    }
                    if (ActiveTabIndex == 1) GridDataAuth?.Reload();
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
        /// chọn tất cả
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected async Task CheckedChangedHandler(bool value, string controlId = nameof(IsCheckAllEvent))
        {
            try
            {
                switch(controlId)
                {
                    case nameof(IsCheckAllEvent):
                        IsCheckAllEvent = value;
                        if (ListMenuAuth.IsNullOrEmpty()) return;
                        await ShowLoading();
                        await Task.Delay(75);
                        ListMenuAuth!.Update(m => m.listEvent?.Update(m => m.isAllow = IsCheckAllEvent));
                        break;
                    case nameof(IsCheckAllData):
                        IsCheckAllData = value;
                        if (ListDataAuth.IsNullOrEmpty()) return;
                        await ShowLoading();
                        await Task.Delay(75);
                        ListDataAuth!.Update(m => m.isAllow = IsCheckAllData);
                        break;
                }    
                
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "CheckedChangedHandler");
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion
    }
}
