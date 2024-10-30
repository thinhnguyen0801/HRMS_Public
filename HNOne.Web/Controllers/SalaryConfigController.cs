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
    public class SalaryConfigController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        #region Properties
        public List<SalaryConfigurationModel>? ListSalaryConfig { get; set; }
        public IGrid? GridSalaryConfig { get; set; }
        public IReadOnlyList<object>? SelectedSalaries { get; set; } = null;
        public SalaryConfigurationModel EntityUpdate { get; set; } = new SalaryConfigurationModel();
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;
        public List<ComboboxModel>? ListCboSalaryCatagory { get; set; } // cbo ds loại lương
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public List<EnumCatagoryModel>? ListCboFormula { get; set; } // cbo ds công thức tính phụ cấp
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Cấu hình thông tin lương", isActive: true)
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
        private async Task getSalaryConfig()
        {
            ListSalaryConfig = new List<SalaryConfigurationModel>();
            ListSalaryConfig = await _masterDataService.GetSalaryConfigAsync(UserId, Token, isShowToast: true);
        }

        private async Task buildComboboxAsync()
        {
            try
            {
                var getTask1 = _masterDataService.GetBranchAsync(UserId, Token);
                var getTask2 = _masterDataService.GetSalaryCatagoryAsync(UserId, Token, "ACTIVE"); // ds loại lương chỉ lấy active
                var getTask3 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.CachTinhLuongPhuCap));
                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask3
                );
                ListCboBranch = (await getTask1)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
                ListCboSalaryCatagory = (await getTask2)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboFormula = await getTask3;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "BuildComboAsync");
            }
        }

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (EntityUpdate.salaryCategoryId < 1)
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Loại lương");
                fieldName = "txtSalaryCategoryId";
                return;
            }
            if (EntityUpdate.branchId < 1)
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chi nhánh");
                fieldName = "txtBranchId";
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

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, SalaryConfigurationModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    EntityUpdate = new SalaryConfigurationModel();
                    if (!ListCboBranch.IsNullOrEmpty()) EntityUpdate.branchId = BranchId;
                }
                else
                {
                    EntityUpdate.id = pItemDetails!.id;
                    EntityUpdate.salaryCategoryId = pItemDetails!.salaryCategoryId;
                    EntityUpdate.branchId = pItemDetails!.branchId;
                    EntityUpdate.isActive = pItemDetails!.isActive;
                    EntityUpdate.isPersonalIncomeTax = pItemDetails!.isPersonalIncomeTax;
                    EntityUpdate.taxLimit = pItemDetails!.taxLimit;
                    EntityUpdate.isSocialInsurance = pItemDetails!.isSocialInsurance;
                    EntityUpdate.isHealthInsurance = pItemDetails!.isHealthInsurance;
                    EntityUpdate.isAccidentInsurance = pItemDetails!.isAccidentInsurance;
                    EntityUpdate.isOccupationalAccidentInsurance = pItemDetails!.isOccupationalAccidentInsurance;
                    EntityUpdate.isUnionFee = pItemDetails!.isUnionFee;
                    EntityUpdate.isOvertime = pItemDetails!.isOvertime;
                    EntityUpdate.overtimeCoefficient = pItemDetails!.overtimeCoefficient;
                    EntityUpdate.isNightShift = pItemDetails!.isNightShift;
                    EntityUpdate.coefficientNightShift = pItemDetails!.coefficientNightShift;
                    EntityUpdate.isAllowance = pItemDetails!.isAllowance;
                    EntityUpdate.isProbationaryPeriod = pItemDetails!.isProbationaryPeriod;
                    EntityUpdate.salaryDefault = pItemDetails!.salaryDefault;
                    EntityUpdate.salaryCalculateMethod = pItemDetails!.salaryCalculateMethod;
                    EntityUpdate.isUseOfGradeLevel = pItemDetails!.isUseOfGradeLevel;
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
                string processKey = IsCreate ? ProcessConstants.POST_SALARY_CONFIG : ProcessConstants.PUT_SALARY_CONFIG;
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
        #endregion
    }
}
