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

namespace HNOne.Web.Controllers
{
    public class WorkConfigController : DocumentControllerBase
    {
        [Inject] IWorkforceService _workforceService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        #region Properties

        public WorkConfigModel WorkConfigUpdate { get; set; } = new WorkConfigModel();
        public List<WorkConfigModel>? ListWorkConfig { get; set; }
        public IGrid? GridWorkConfig { get; set; }

        public List<ComboboxModel>? ListCboYear { get; set; }
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
                    //string errMessage = await CheckMenuPermissionAsync("chi-nhanh");
                    //if (errMessage == "401") return; // kiểm quyền menu page danh sách

                    //await checkPermission(errMessage);
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
                    var header = lstResult!.First(m=>m.workConfigType == "DEFAULT");
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
                    ListWorkConfig = lstResult!.Where(m => m.workConfigType != "DEFAULT").ToList();
                }    
            }
            catch (Exception) { throw; }
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

        protected async Task SaveDataHandler()
        {
            try
            {
                
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
                bool isConfirm = true;
                string errorMessage = string.Empty;
                errorMessage = $"Bạn có chắc muốn phát sinh thông tin kỳ công chi tiết năm {WorkConfigUpdate.year} không?";
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
                isConfirm = await _workforceService.UpdateMasterDataAsync(request);
                if (isConfirm) await getConfigAsync();
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
        #endregion
    }
}
