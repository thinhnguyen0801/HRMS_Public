using HNOne.Web.Components.Controls;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HNOne.Model.Models;
using DevExpress.Blazor;
using HNOne.Model;
using HNOne.Common;
using HNOne.Web.Commons;
using DevExpress.Pdf.Native.BouncyCastle.Asn1.Ocsp;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class WorkConfigController : DocumentControllerBase
    {
        [Inject] IWorkforceService _workforceService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        const string STRING_KEY_EVENT_POST = "WORK_CONFIG_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "WORK_CONFIG_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "WORK_CONFIG_CONTROLLER_DELETE";
        #region Properties

        public WorkConfigModel WorkConfigUpdate { get; set; } = new WorkConfigModel();
        public List<WorkConfigModel>? ListWorkConfig { get; set; }
        public IGrid? GridWorkConfig { get; set; }

        public List<ComboboxModel>? ListCboYear { get; set; }

        public bool IsShowDialogPreview { get; set; }
        public List<WorkConfigModel>? ListWorkConfigPreView { get; set; }
        public IGrid? GridWorkConfigPreview { get; set; }
        // nút quyền
        public bool IsAllowPost { get; set; }
        public bool IsAllowPut { get; set; }
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    string errMessage = await CheckMenuPermissionAsync("cau-hinh-thong-so-cong");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await checkPermission(errMessage);
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Thông số công", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    //
                    initDataAsync();
                    await getConfigAsync();
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
        private void initDataAsync()
        {
            WorkConfigUpdate.year = DateTime.Now.Year;
            int defaultYear = 2024;
            ListCboYear = new List<ComboboxModel>();
            for (int i = defaultYear; i < DateTime.Now.AddYears(2).Year; i++)
            {
                ListCboYear.Add(new ComboboxModel() { id = i, name = $"Năm {i}" });
            }
        }
        private async Task getConfigAsync()
        {
            try
            {
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.process = ProcessConstants.GET_WORK_CONFIG;
                request.opt = WorkConfigUpdate.year.ToString();
                var lstResult = await _workforceService.GetMasterDataAsync<WorkConfigModel>(request, isShowToast: true);
                if(!lstResult.IsNullOrEmpty())
                {
                    var header = lstResult!.First(m=>m.workConfigType == CommonConstants.WORK_TYPE_DEFAULT);
                    WorkConfigUpdate.startDate = header.startDate;
                    WorkConfigUpdate.closingDate = header.closingDate;
                    WorkConfigUpdate.closingDate1 = header.closingDate1;
                    WorkConfigUpdate.isLastDayOfMonth = header.isLastDayOfMonth;
                    WorkConfigUpdate.totalWorkingDayOfMonth = header.totalWorkingDayOfMonth;
                    WorkConfigUpdate.isWorkingDayExcludeDayOff = header.isWorkingDayExcludeDayOff;
                    WorkConfigUpdate.totalWorkingHours = header.totalWorkingHours;
                    WorkConfigUpdate.symbolOfWeekdayDayOff = header.symbolOfWeekdayDayOff;
                    WorkConfigUpdate.bgColorOfWeekdayDayOff = header.bgColorOfWeekdayDayOff;
                    WorkConfigUpdate.symbolOfHoliday = header.symbolOfHoliday;
                    WorkConfigUpdate.bgColorOfHoliday = header.bgColorOfHoliday;
                    WorkConfigUpdate.symbolWorkingDay = header.symbolWorkingDay;
                    WorkConfigUpdate.symbolOfUnpaidLeave = header.symbolOfUnpaidLeave;
                    WorkConfigUpdate.bgColorOfUnpaidLeave = header.bgColorOfUnpaidLeave;
                    WorkConfigUpdate.symbolOfOvertime = header.symbolOfOvertime;
                    WorkConfigUpdate.bgColorOfOvertime = header.bgColorOfOvertime;
                    WorkConfigUpdate.symbolOfLeaveOfAbsence = header.symbolOfLeaveOfAbsence;
                    WorkConfigUpdate.bgColorOfLeaveOfAbsence = header.bgColorOfLeaveOfAbsence;
                    ListWorkConfig = lstResult!.Where(m => m.workConfigType != CommonConstants.WORK_TYPE_DEFAULT).ToList();
                }    
            }
            catch (Exception) { throw; }
        }

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if(WorkConfigUpdate.startDate < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ngày b/đ chấm công");
                fieldName = "txtstartDate";
                return;
            }
            if (WorkConfigUpdate.closingDate < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ngày chốt kỳ công");
                fieldName = "txtclosingDate";
                return;
            }
            if (WorkConfigUpdate.isLastDayOfMonth == false && WorkConfigUpdate.closingDate1 < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ngày k/t chấm công");
                fieldName = "txtclosingDate1";
                return;
            }
            if (WorkConfigUpdate.isWorkingDayExcludeDayOff == false && WorkConfigUpdate.totalWorkingDayOfMonth < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Số ngày làm việc");
                fieldName = "txttotalWorkingDayOfMonth";
                return;
            }
            if (WorkConfigUpdate.totalWorkingHours < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Số giờ làm việc");
                fieldName = "txttotalWorkingHours";
                return;
            }
            if (string.IsNullOrWhiteSpace(WorkConfigUpdate.symbolWorkingDay))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ký hiệu ngày làm việc");
                fieldName = "txtsymbolWorkingDay";
                return;
            }
            if (string.IsNullOrWhiteSpace(WorkConfigUpdate.symbolOfWeekdayDayOff))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ký hiệu ngày nghỉ trong tuần");
                fieldName = "txtsymbolOfWeekdayDayOff";
                return;
            }
            if (string.IsNullOrWhiteSpace(WorkConfigUpdate.symbolOfHoliday))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ký hiệu ngày nghỉ lễ");
                fieldName = "txtsymbolOfHoliday";
                return;
            }
            if (string.IsNullOrWhiteSpace(WorkConfigUpdate.symbolOfLeaveOfAbsence))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ký hiệu ngày nghỉ phép");
                fieldName = "symbolOfLeaveOfAbsence";
                return;
            }
            if (string.IsNullOrWhiteSpace(WorkConfigUpdate.symbolOfUnpaidLeave))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ký hiệu ngày nghỉ phép không lương");
                fieldName = "symbolOfUnpaidLeave";
                return;
            }
            if (string.IsNullOrWhiteSpace(WorkConfigUpdate.symbolOfOvertime))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ký hiệu ngày tăng ca");
                fieldName = "symbolOfUnpaidLeave";
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
            IsAllowPut = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_PUT) != null;
        }
        #endregion

        #region Protected Functions
        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getConfigAsync();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "RefreshHandler");
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
        /// lưu thông tin cấu hình
        /// </summary>
        /// <returns></returns>
        protected async Task SaveDataHandler()
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
                string fieldName = string.Empty;
                validateForSave(ref errorMessage, ref fieldName);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                    return;
                }
                errorMessage = string.Format(MessageConstants.MESSAGE_CONFIRM_UPDATE_FORMAT, $"Cấu hình thông số công mặc định");
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                if (WorkConfigUpdate.isLastDayOfMonth) WorkConfigUpdate.closingDate1 = 0;
                if (WorkConfigUpdate.isWorkingDayExcludeDayOff) WorkConfigUpdate.totalWorkingDayOfMonth = 0;
                WorkConfigUpdate.userSign = UserId;
                WorkConfigUpdate.userSign2 = UserId;
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.token = Token;
                request.branchId = BranchId;
                request.process = ProcessConstants.PUT_WORK_CONFIG;
                request.json = JsonConvert.SerializeObject(WorkConfigUpdate);
                isConfirm = await _workforceService.UpdateMasterDataAsync(request);
                if(isConfirm)
                {
                    await getConfigAsync();
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
        /// phát sinh kỳ công chi tiết năm được chọn
        /// </summary>
        /// <returns></returns>
        protected async Task GenerateWorkHandler()
        {
            try
            {
                await checkPermission(MenuId);
                if (!IsAllowPost)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
                bool isConfirm = true;
                string errorMessage = string.Empty;
                errorMessage = $"Bạn có chắc muốn lưu thông tin kỳ công chi tiết năm {WorkConfigUpdate.year} không?";
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.POST_WORK_CONFIG;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.opt = WorkConfigUpdate.year.ToString();
                request.type = "INSERT";
                isConfirm = await _workforceService.UpdateMasterDataAsync(request);
                if (isConfirm)
                {
                    await getConfigAsync();
                    IsShowDialogPreview = false;
                }
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "GenerateWorkHandler");
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
        /// Xem trước qua dữ liệu chấm công
        /// </summary>
        /// <returns></returns>
        protected async Task PreviewWorkConfigHandler()
        {
            try
            {
                await ShowLoading();
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.POST_WORK_CONFIG;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.opt = WorkConfigUpdate.year.ToString();
                request.type = "PREVIEW";
                var result = await _workforceService.GenerateWorkConfigAsync(request);
                if (result.IsNullOrEmpty()) return;
                ListWorkConfigPreView = result;
                IsShowDialogPreview = true;
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "PreviewWorkConfigHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(75);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion
    }
}
