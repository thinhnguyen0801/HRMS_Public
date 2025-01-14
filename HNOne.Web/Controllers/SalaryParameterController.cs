using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class SalaryParameterController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        const string STRING_KEY_EVENT_POST = "SALARY_PARAMETER_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "SALARY_PARAMETER_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "SALARY_PARAMETER_CONTROLLER_DELETE";

        #region Properties
        public List<SalaryParameterModel>? ListSalaryConfig { get; set; }
        public IGrid? GridSalaryConfig { get; set; }
        public IReadOnlyList<object>? SelectedSalaries { get; set; } = null;
        public SalaryParameterModel EntityUpdate { get; set; } = new SalaryParameterModel();
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;

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
                    string errMessage = await CheckMenuPermissionAsync("thong-so-luong");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Lương"),
                        new BreadcrumbModel("Thông số lương", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await buildComboboxAsync();
                    await getSalaryConfig();

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
                var getTask1 = _masterDataService.GetBranchAsync(UserId, Token);
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

        private async Task getSalaryConfig()
        {
            ListSalaryConfig = new List<SalaryParameterModel>();
            ListSalaryConfig = await _masterDataService.GetSalaryParameterAsync(UserId, Token, BranchId, $"{BranchIds}", isShowToast: true);
        }

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (EntityUpdate.branchId < 1)
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chi nhánh");
                fieldName = "txtBranchId";
                return;
            }
            if (!EntityUpdate.fromDate.HasValue)
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Từ ngày");
                fieldName = "toDate";
                return;
            }
            if (!EntityUpdate.toDate.HasValue)
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Đến ngày");
                fieldName = "endDate";
                return;
            }
            if (EntityUpdate.toDate.Value.Date < EntityUpdate.fromDate.Value.Date)
            {
                errorMessage = MessageConstants.MESSAGE_FROM_DATE_TO_DATE_INVALID;
                fieldName = "endDate";
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

        #region Protected Functions
        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getSalaryConfig();
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

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, SalaryParameterModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    EntityUpdate = new SalaryParameterModel();
                    EntityUpdate.isActive = true;
                    if (!ListCboBranch.IsNullOrEmpty()) EntityUpdate.branchId = BranchId;
                }
                else
                {
                    EntityUpdate.id = pItemDetails!.id;
                    EntityUpdate.branchId = pItemDetails!.branchId;
                    EntityUpdate.branchName = pItemDetails!.branchName;
                    EntityUpdate.isActive = pItemDetails!.isActive;
                    EntityUpdate.taxSalary = pItemDetails!.taxSalary;
                    EntityUpdate.taxSalaryProbationary = pItemDetails!.taxSalaryProbationary;
                    EntityUpdate.salaryFamilyCircumstanceDeduction = pItemDetails!.salaryFamilyCircumstanceDeduction;
                    EntityUpdate.fromDate = pItemDetails!.fromDate;
                    EntityUpdate.toDate = pItemDetails!.toDate;
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
                string processKey = IsCreate ? ProcessConstants.POST_SALARY_PARAMETER : ProcessConstants.PUT_SALARY_PARAMETER;
                EntityUpdate.userSign = UserId;
                EntityUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(EntityUpdate);
                isConfirm = await _masterDataService.UpdateSalaryConfigAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getSalaryConfig();
                    IsShowDialog = false;
                    SelectedSalaries = null;
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
        /// Kết xuất dữ liệu sang file excel
        /// xlsx
        /// </summary>
        /// <returns></returns>
        protected async Task ExportExcelHandler()
        {
            try
            {
                if (GridSalaryConfig == null || ListSalaryConfig.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                await ShowLoading();
                await GridSalaryConfig!.ExportToXlsxAsync("Thong-so-luong", new GridXlExportOptions()
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
