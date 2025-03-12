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
    public class HolidayCatagoryController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IWorkforceService _workforceService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        const string STRING_KEY_EVENT_POST = "HOLIDAY_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "HOLIDAY_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "HOLIDAY_CONTROLLER_DELETE";

        #region Properties
        public List<HolidayCatagoryModel>? ListHoliday { get; set; }
        public IGrid? GridHoliday { get; set; }
        public IReadOnlyList<object>? SelectedItems { get; set; } = null;
        public HolidayCatagoryModel HolidayUpdate { get; set; } = new HolidayCatagoryModel();

        public List<EnumCatagoryModel>? ListCboType { get; set; } // cbo ds loại ngày nghỉ
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
                    string errMessage = await CheckMenuPermissionAsync("danh-muc-ngay-nghi");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Danh mục ngày nghỉ phép", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await buildComboboxAsync();
                    await getHolidayCatagory();

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
                var getTask1 = _masterDataService.GetBranchAsync(UserId, Token, BranchId, $"{BranchIds}", supperAdmin: IsAdmin ? "Y" : "N");
                var getTask2 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.LoaiNgayNghi)); // ds trạng thái
                await Task.WhenAll(
                    getTask1,
                    getTask2
                );
                ListCboBranch = (await getTask1)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
                ListCboType = await getTask2;
            }
            catch (Exception ex)
            {
                throw ex;
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

        private async Task getHolidayCatagory()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.branchId = BranchId;
            request.process = ProcessConstants.GET_HOILDAY_CATAGORY;
            request.branchIds = $"{BranchIds}";
            ListHoliday = new List<HolidayCatagoryModel>();
            ListHoliday = await _workforceService.GetMasterDataAsync<HolidayCatagoryModel>(request, isShowToast: true);
        }

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(HolidayUpdate.type))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Loại ngày nghỉ");
                fieldName = "txtType";
                return;
            }
            if (string.IsNullOrEmpty(HolidayUpdate.name))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Tên ngày nghỉ");
                fieldName = "txtName";
                return;
            }
            if (HolidayUpdate.toDate.Date < HolidayUpdate.fromDate.Date)
            {
                errorMessage = MessageConstants.MESSAGE_FROM_DATE_TO_DATE_INVALID;
                fieldName = "endDate";
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
                await getHolidayCatagory();
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

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, HolidayCatagoryModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    HolidayUpdate = new HolidayCatagoryModel();
                    HolidayUpdate.fromDate = DateTime.Now;
                    HolidayUpdate.toDate = DateTime.Now;
                    HolidayUpdate.branchId = BranchId;
                }
                else
                {
                    HolidayUpdate.id = pItemDetails!.id;
                    HolidayUpdate.name = pItemDetails!.name;
                    HolidayUpdate.branchId = pItemDetails!.branchId;
                    HolidayUpdate.fromDate = pItemDetails!.fromDate;
                    HolidayUpdate.toDate = pItemDetails!.toDate;
                    HolidayUpdate.color = pItemDetails!.color;
                    HolidayUpdate.color = pItemDetails!.color;
                    HolidayUpdate.type = pItemDetails!.type;
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
                HolidayUpdate.userSign = UserId;
                HolidayUpdate.userSign2 = UserId;
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.token = Token;
                request.branchId = BranchId;
                request.process = IsCreate ? ProcessConstants.POST_HOILDAY_CATAGORY : ProcessConstants.PUT_HOILDAY_CATAGORY;
                request.json = JsonConvert.SerializeObject(HolidayUpdate);
                isConfirm = await _workforceService.UpdateMasterDataAsync(request);
                if (isConfirm)
                {
                    await getHolidayCatagory();
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
                string tableName = _encryptHelper.Encrypt(nameof(EnumObjType.HolidayCatagories));
                string pKey = _encryptHelper.Encrypt(nameof(HolidayCatagoryModel.id));
                string fKey = _encryptHelper.Encrypt(nameof(LeaveRequest1Model.holidayId));
                string ids = string.Join(",", SelectedItems!.Cast<HolidayCatagoryModel>().Select(m => m.id));
                string reasonDelete = "";
                string strResult = await _masterDataService.DeleteDynnamicAsync(UserId, Token, BranchId, tableName, pKey, fKey, ids, reasonDelete);
                if (strResult == "-1") return;
                if (strResult == StatusCodes.Status200OK.ToString())
                {
                    await getHolidayCatagory();
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
                if (GridHoliday == null || ListHoliday.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                await ShowLoading();
                await GridHoliday!.ExportToXlsxAsync("Danh-muc-ngay-nghi", new GridXlExportOptions()
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
