using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using HNOne.Web.Models;
using HNOne.Web.Services;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class BranchController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        const string STRING_KEY_EVENT_POST = "BRANCH_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "BRANCH_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "BRANCH_CONTROLLER_DELETE";
        #region Properties
        public List<BranchModel>? ListBranch { get; set; }
        public IGrid? GridBranch { get; set; }
        public IReadOnlyList<object>? SelectedBranchs { get; set; } = null;
        public BranchModel BranchUpdate { get; set; } = new BranchModel();
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;
        public bool IsShowPassword { get; set; } = false;

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
                    string errMessage = await CheckMenuPermissionAsync("chi-nhanh");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Chi nhánh", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await getBranchs();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OnAfterRenderAsync");
                    ShowError(ex.Message);
                }
                finally
                {
                    await ShowLoading(false);
                    //await _progressService!.Done();
                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        #region Private Functions
        private async Task getBranchs()
        {
            ListBranch = new List<BranchModel>();
            ListBranch = await _masterDataService.GetBranchAsync(UserId, Token);
        }
        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(BranchUpdate.branchCode))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Mã chi nhánh");
                fieldName = nameof(BranchUpdate.branchCode);
                return;
            }
            if (string.IsNullOrEmpty(BranchUpdate.branchName))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Tên chi nhánh");
                fieldName = nameof(BranchUpdate.branchName);
                return;
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
        #endregion

        #region
        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getBranchs();
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
        
        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, BranchModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    BranchUpdate = new BranchModel();
                }
                else
                {
                    BranchUpdate.branchId = pItemDetails!.branchId;
                    BranchUpdate.branchCode = pItemDetails!.branchCode;
                    BranchUpdate.branchName = pItemDetails!.branchName;
                    BranchUpdate.phoneNumber = pItemDetails!.phoneNumber;
                    BranchUpdate.imgUrl = pItemDetails!.imgUrl;
                    BranchUpdate.address = pItemDetails!.address;
                    BranchUpdate.defaultPassword = _encryptHelper.Decrypt(pItemDetails!.defaultPassword);
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
                string processKey = IsCreate ? ProcessConstants.POST_BRANCH : ProcessConstants.PUT_BRANCH;
                BranchUpdate.userSign = UserId;
                BranchUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(BranchUpdate);
                isConfirm = await _masterDataService.UpdateBranchAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getBranchs();
                    IsShowDialog = false;
                    SelectedBranchs = null;
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
        public async Task DeleteDataHandler()
        {
            try
            {
                await checkPermission(MenuId);
                if(!IsAllowDelete)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }    
                if (SelectedBranchs.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                await ShowLoading();
                string tableName = _encryptHelper.Encrypt(nameof(EnumObjType.Branchs));
                string pKey = _encryptHelper.Encrypt(nameof(BranchModel.branchId));
                string fKey = _encryptHelper.Encrypt(nameof(BranchModel.branchId));
                string ids = string.Join(",", SelectedBranchs!.Cast<BranchModel>().Select(m => m.branchId));
                string reasonDelete = "";
                string strResult = await _masterDataService.DeleteDynnamicAsync(UserId, Token, BranchId, tableName, pKey, fKey, ids, reasonDelete);
                if (strResult == "-1") return;
                if (strResult == StatusCodes.Status200OK.ToString())
                {
                    await getBranchs();
                    SelectedBranchs = null;
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

        protected void ShowPasswordHandler() => IsShowPassword = !IsShowPassword;
        #endregion
    }
}
