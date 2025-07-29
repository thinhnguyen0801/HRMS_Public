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
    public class ReasonCategorieController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        const string STRING_KEY_EVENT_POST = "REASON_CATAGORY_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "REASON_CATAGORY_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "REASON_CATAGORY_CONTROLLER_DELETE";
        #region Properties
        public List<ReasonCategorieModel>? ListReasonCategory { get; set; }
        public IGrid? GridReasonCategory { get; set; }
        public IReadOnlyList<object>? SelectedReasonCategories { get; set; } = null;
        public ReasonCategorieModel ReasonCategorieUpdate { get; set; } = new ReasonCategorieModel();
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;
        public List<EnumCatagoryModel>? ListCboType { get; set; } // cbo ds loại lý do
        public List<EnumCatagoryModel>? ListCboLeaveRequest { get; set; } // cbo ds loại nghỉ phép
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
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
                    string errMessage = await CheckMenuPermissionAsync("ly-do");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Lý do", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await buildComboAsync();
                    await getReasonCategories();

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
        private async Task buildComboAsync()
        {
            var getTask1 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.LoaiLyDo));
            var getTask2 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.LoaiNghiPhep));
            var getTask3 = _masterDataService.GetBranchAsync(UserId, Token, BranchId, $"{BranchIds}", supperAdmin: IsAdmin ? "Y" : "N");
            await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask3
                );
            ListCboType = await getTask1;
            ListCboLeaveRequest = await getTask2;
            ListCboBranch = (await getTask3)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
        }
        private async Task getReasonCategories()
        {
            ListReasonCategory = new List<ReasonCategorieModel>();
            ListReasonCategory = await _masterDataService.GetReasonCategoryAsync(UserId, Token, BranchId, $"{BranchIds}");
        }
        
        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(ReasonCategorieUpdate.name))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Tên lý do");
                fieldName = "txtName";
                return;
            }
            if (ReasonCategorieUpdate.branchId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chi nhánh");
                fieldName = "txtBranchId";
                return;
            }
            if (string.IsNullOrEmpty(ReasonCategorieUpdate.type))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Loại lý do");
                fieldName = "txtType";
                return;
            }
            if (ReasonCategorieUpdate.type == GlobalContants.ENUM_REASON_DNNP)
            {
                if(string.IsNullOrEmpty(ReasonCategorieUpdate.value2))
                {
                    errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Loại nghỉ phép");
                    fieldName = "txtvalue2";
                    return;
                }
                if (string.IsNullOrEmpty(ReasonCategorieUpdate.symbolDayOff))
                {
                    errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Ký hiệu ngày nghỉ");
                    fieldName = "symbolDayOff";
                    return;
                }
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
                await getReasonCategories();
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

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, ReasonCategorieModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    ReasonCategorieUpdate = new ReasonCategorieModel();
                    ReasonCategorieUpdate.branchId = BranchId;
                }
                else
                {
                    ReasonCategorieUpdate.id = pItemDetails!.id;
                    ReasonCategorieUpdate.name = pItemDetails!.name;
                    ReasonCategorieUpdate.branchId = pItemDetails!.branchId;
                    ReasonCategorieUpdate.type = pItemDetails!.type;
                    ReasonCategorieUpdate.value2 = pItemDetails.value2;
                    ReasonCategorieUpdate.symbolDayOff = pItemDetails.symbolDayOff;
                    ReasonCategorieUpdate.bgColorDayOff = pItemDetails.bgColorDayOff;
                    decimal.TryParse(pItemDetails!.value, out decimal oConfig);
                    decimal.TryParse(pItemDetails!.value1, out decimal oConfig1);
                    ReasonCategorieUpdate.isActive = pItemDetails!.isActive;
                    ReasonCategorieUpdate.config = oConfig;
                    ReasonCategorieUpdate.config1 = oConfig1;
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
                string processKey = IsCreate ? ProcessConstants.POST_REASONCATEGORIE : ProcessConstants.PUT_REASONCATEGORIE;
                ReasonCategorieUpdate.userSign = UserId;
                ReasonCategorieUpdate.userSign2 = UserId;
                ReasonCategorieUpdate.value = ReasonCategorieUpdate.config.ToString();
                ReasonCategorieUpdate.value1 = ReasonCategorieUpdate.config1.ToString();
                ReasonCategorieUpdate.value2 = ReasonCategorieUpdate.type == GlobalContants.ENUM_REASON_DNNP 
                    ? ReasonCategorieUpdate.value2 : ReasonCategorieUpdate.config2.ToString();
                string content = JsonConvert.SerializeObject(ReasonCategorieUpdate);
                isConfirm = await _masterDataService.UpdateReasonCategorieAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getReasonCategories();
                    IsShowDialog = false;
                    SelectedReasonCategories = null;
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
        /// Xóa thông tin lý do
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
                if (SelectedReasonCategories.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                await ShowLoading();
                string tableName = _encryptHelper.Encrypt(nameof(EnumObjType.ReasonCategories));
                string pKey = _encryptHelper.Encrypt(nameof(ReasonCategorieModel.id));
                string fKey = _encryptHelper.Encrypt(nameof(LeaveRequestModel.reasonId));
                string ids = string.Join(",", SelectedReasonCategories!.Cast<ReasonCategorieModel>().Select(m => m.id));
                string reasonDelete = "";
                string strResult = await _masterDataService.DeleteDynnamicAsync(UserId, Token, BranchId, tableName, pKey, fKey, ids, reasonDelete);
                if (strResult == "-1") return;
                if (strResult == StatusCodes.Status200OK.ToString())
                {
                    await getReasonCategories();
                    SelectedReasonCategories = null;
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
                if (GridReasonCategory == null || ListReasonCategory.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                await ShowLoading();
                await GridReasonCategory!.ExportToXlsxAsync("Danh-muc-ly-do", new GridXlExportOptions()
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
