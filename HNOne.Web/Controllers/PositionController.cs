using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
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
    public class PositionController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        const string STRING_KEY_EVENT_POST = "POSITION_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "POSITION_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "POSITION_CONTROLLER_DELETE";

        #region Properties
        public List<PositionModel>? ListPosition { get; set; }
        public IGrid? GridPosition { get; set; }
        public IReadOnlyList<object>? SelectedPositions { get; set; } = null;
        public PositionModel PositionUpdate { get; set; } = new PositionModel();
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public List<EnumCatagoryModel>? ListCboLevel { get; set; } // cbo ds trạng thái nhân viên

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
                    string errMessage = await CheckMenuPermissionAsync("chuc-vu");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Chức vụ", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await buildComboboxAsync();
                    await getPositions();

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
        private async Task getPositions()
        {
            ListPosition = new List<PositionModel>();
            ListPosition = await _masterDataService.GetPositionAsync(UserId, Token);
        }

        private async Task buildComboboxAsync()
        {
            try
            {
                var getTask1 = _masterDataService.GetBranchAsync(UserId, Token);
                var getTask2 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.CapDoNhanVien)); // cấp độ nhân viên
                await Task.WhenAll(
                    getTask1,
                    getTask2
                );
                ListCboBranch = (await getTask1)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
                ListCboLevel = await getTask2;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "BuildComboAsync");
            }
        }

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(PositionUpdate.name))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Tên chức vụ");
                fieldName = "txtName";
                return;
            }
            if (PositionUpdate.branchId < 1)
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chi nhánh");
                fieldName = "txtBranchId";
                return;
            }
            if (string.IsNullOrEmpty(PositionUpdate.levelCode))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Cấp độ");
                fieldName = "txtLevelCode";
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
                await getPositions();
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

        protected async Task OnOpenDialogHandler(EnumType pAction = EnumType.Add, PositionModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    PositionUpdate = new PositionModel();
                    if (!ListCboBranch.IsNullOrEmpty()) PositionUpdate.branchId = BranchId;
                }
                else
                {
                    PositionUpdate.id = pItemDetails!.id;
                    PositionUpdate.code = pItemDetails!.code;
                    PositionUpdate.name = pItemDetails!.name;
                    PositionUpdate.remark = pItemDetails!.remark;
                    PositionUpdate.isActive = pItemDetails!.isActive;
                    PositionUpdate.branchId = pItemDetails!.branchId;
                    PositionUpdate.levelCode = pItemDetails!.levelCode;
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
                string processKey = IsCreate ? ProcessConstants.POST_POSITION : ProcessConstants.PUT_POSITION;
                PositionUpdate.userSign = UserId;
                PositionUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(PositionUpdate);
                isConfirm = await _masterDataService.UpdatePositionAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getPositions();
                    IsShowDialog = false;
                    SelectedPositions = null;
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
        /// xóa dữ liệu chức vụ
        /// </summary>
        /// <returns></returns>
        public async Task DeleteDataHandler()
        {
            try
            {
                await checkPermission(MenuId);
                if (!IsAllowDelete)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
                if (SelectedPositions.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                await ShowLoading();
                string tableName = _encryptHelper.Encrypt(nameof(EnumObjType.Positions));
                string pKey = _encryptHelper.Encrypt(nameof(PositionModel.id));
                string fKey = _encryptHelper.Encrypt(nameof(EmployeeModel.positionId));
                string ids = string.Join(",", SelectedPositions!.Cast<PositionModel>().Select(m => m.id));
                string reasonDelete = "";
                string strResult = await _masterDataService.DeleteDynnamicAsync(UserId, Token, BranchId, tableName, pKey, fKey, ids, reasonDelete);
                if (strResult == "-1") return;
                if (strResult == StatusCodes.Status200OK.ToString())
                {
                    await getPositions();
                    SelectedPositions = null;
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
        #endregion
    }
}
