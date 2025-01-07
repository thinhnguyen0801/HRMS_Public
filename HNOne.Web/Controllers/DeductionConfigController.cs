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
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class DeductionConfigController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        const string STRING_KEY_EVENT_POST = "TAXT_RATE_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "TAXT_RATE_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "TAXT_RATE_CONTROLLER_DELETE";

        #region Properties
        public List<DeductionConfigModel>? ListDeduction { get; set; }
        public IGrid? GridDeduction { get; set; }
        public IReadOnlyList<object>? SelectedItems { get; set; } = null;
        public DeductionConfigModel DeductionUpdate { get; set; } = new DeductionConfigModel();
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;

        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public List<EnumCatagoryModel>? ListCboType { get; set; } // cbo ds chi nhánh
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
                    string errMessage = await CheckMenuPermissionAsync("danh-muc-muc-thue");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Lương"),
                        new BreadcrumbModel("Danh mục mức thuế", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await buildComboboxAsync();
                    await getDeductionConfig();
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

        private async Task buildComboboxAsync()
        {
            try
            {
                var getTask1 = _masterDataService.GetBranchAsync(UserId, Token);
                var getTask2 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.TrichNop)); // ds trích nộp
                await Task.WhenAll(
                    getTask1,
                    getTask2
                    );
                ListCboBranch = (await getTask1)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
                ListCboType = await getTask2;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "BuildComboAsync");
            }
        }

        private async Task getDeductionConfig()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.branchId = BranchId;
            request.process = ProcessConstants.GET_DEDUCTION_CONFIG;
            ListDeduction = new List<DeductionConfigModel>();
            ListDeduction = await _masterDataService.GetMasterAsync<DeductionConfigModel>(request, isShowToast: true);
        }

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (DeductionUpdate.branchId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chi nhánh");
                fieldName = "txtBranchId";
                return;
            }
            if (string.IsNullOrEmpty(DeductionUpdate.type))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Loại trích nộp");
                fieldName = "txtType";
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
                await getDeductionConfig();
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

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, DeductionConfigModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    DeductionUpdate = new DeductionConfigModel();
                }
                else
                {
                    DeductionUpdate.id = pItemDetails!.id;
                    DeductionUpdate.branchId = pItemDetails!.branchId;
                    DeductionUpdate.type = pItemDetails!.type;
                    DeductionUpdate.coefficientEnterprise = pItemDetails!.coefficientEnterprise;
                    DeductionUpdate.coefficientEmployee = pItemDetails!.coefficientEmployee;
                    DeductionUpdate.isActive = pItemDetails!.isActive;
                    DeductionUpdate.fromDate = pItemDetails!.fromDate;
                    DeductionUpdate.toDate = pItemDetails!.toDate;
                    DeductionUpdate.maxEnterprise = pItemDetails!.maxEnterprise;
                    DeductionUpdate.maxEmployee = pItemDetails!.maxEmployee;
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
                DeductionUpdate.userSign = UserId;
                DeductionUpdate.userSign2 = UserId;
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.token = Token;
                request.branchId = BranchId;
                request.process = IsCreate ? ProcessConstants.POST_DEDUCTION_CONFIG : ProcessConstants.PUT_DEDUCTION_CONFIG;
                request.json = JsonConvert.SerializeObject(DeductionUpdate);
                isConfirm = await _masterDataService.UpdateMasterAsync(request);
                if (isConfirm)
                {
                    await getDeductionConfig();
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
                string tableName = _encryptHelper.Encrypt(nameof(EnumObjType.DeductionConfigs));
                string pKey = _encryptHelper.Encrypt(nameof(DeductionConfigModel.id));
                string fKey = _encryptHelper.Encrypt("deductionConfigId");
                string ids = string.Join(",", SelectedItems!.Cast<DeductionConfigModel>().Select(m => m.id));
                string reasonDelete = "";
                string strResult = await _masterDataService.DeleteDynnamicAsync(UserId, Token, BranchId, tableName, pKey, fKey, ids, reasonDelete);
                if (strResult == "-1") return;
                if (strResult == StatusCodes.Status200OK.ToString())
                {
                    await getDeductionConfig();
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
        /// Kết xuất dữ liệu sang file excel
        /// xlsx
        /// </summary>
        /// <returns></returns>
        protected async Task ExportExcelHandler()
        {
            try
            {
                if (GridDeduction == null || ListDeduction.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                await ShowLoading();
                await GridDeduction!.ExportToXlsxAsync("Cau-hinh-trich-nop", new GridXlExportOptions()
                {
                    ExportTotalSummaries = false,
                    ExportGroupSummaries = false
                });
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ExportExcelHandler");
                ShowError(ex.Message);
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
