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

        const string STRING_KEY_EVENT_POST = "SALARY_CONFIG_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "SALARY_CONFIG_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "SALARY_CONFIG_CONTROLLER_DELETE";
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
                    string errMessage = await CheckMenuPermissionAsync("cau-hinh-tinh-luong");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
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
            ListSalaryConfig = await _masterDataService.GetSalaryConfigAsync(UserId, Token, BranchId, $"{BranchIds}", isShowToast: true);
        }

        private async Task buildComboboxAsync()
        {
            try
            {
                var getTask1 = _masterDataService.GetBranchAsync(UserId, Token, BranchId, $"{BranchIds}", supperAdmin: IsAdmin ? "Y" : "N");
                //var getTask2 = _masterDataService.GetSalaryCatagoryAsync(UserId, Token, "ACTIVE"); // ds loại lương chỉ lấy active
                var getTask2 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.LoaiLuong)); // ds loại lương
                var getTask3 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.CachTinhLuongPhuCap));
                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask3
                );
                ListCboBranch = (await getTask1)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
                ListCboSalaryCatagory = (await getTask2)?.Select(m => new ComboboxModel() { code = m.code, name = m.name })?.ToList();
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
            //if (EntityUpdate.salaryCategoryId < 1)
            //{
            //    errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Loại lương");
            //    fieldName = "txtSalaryCategoryId";
            //    return;
            //}
            if (string.IsNullOrEmpty(EntityUpdate.salaryCategoryCode))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Mã loại lương");
                fieldName = "salaryCategoryCode";
                return;
            }
            if (string.IsNullOrEmpty(EntityUpdate.salaryCategoryName))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Tên loại lương");
                fieldName = "salaryCategoryCode";
                return;
            }
            if (EntityUpdate.branchId < 1)
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chi nhánh");
                fieldName = "txtBranchId";
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
                    EntityUpdate.salaryCategoryCode = pItemDetails!.salaryCategoryCode;
                    EntityUpdate.salaryCategoryName = pItemDetails!.salaryCategoryName;
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
                    EntityUpdate.isPrintContract = pItemDetails!.isPrintContract;
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

        /// <summary>
        /// Xóa thông tin lương
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
                if (SelectedSalaries.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                await ShowLoading();
                string tableName = _encryptHelper.Encrypt(nameof(EnumObjType.SalaryConfigurations));
                string pKey = _encryptHelper.Encrypt(nameof(SalaryConfigurationModel.id));
                string fKey = _encryptHelper.Encrypt(nameof(SalaryConfigurationModel.salaryCategoryId));
                string ids = string.Join(",", SelectedSalaries!.Cast<SalaryConfigurationModel>().Select(m => m.id));
                string reasonDelete = "";
                string strResult = await _masterDataService.DeleteDynnamicAsync(UserId, Token, BranchId, tableName, pKey, fKey, ids, reasonDelete);
                if (strResult == "-1") return;
                if (strResult == StatusCodes.Status200OK.ToString())
                {
                    await getSalaryConfig();
                    SelectedSalaries = null;
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
                if (GridSalaryConfig == null || ListSalaryConfig.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                await ShowLoading();
                await GridSalaryConfig!.ExportToXlsxAsync("Cau-hinh-tinh-luong", new GridXlExportOptions()
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
