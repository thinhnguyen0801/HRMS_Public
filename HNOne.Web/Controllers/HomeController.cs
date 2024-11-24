using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Models;
using HNOne.Web.Services;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;

namespace HNOne.Web.Controllers
{
    public class HomeController : DocumentControllerBase
    {
        [Inject] IWorkforceService _workforceService { get; init; }
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] private IJSRuntime _jsRuntime { get; init; }
        #region Properties
        public DateTime StartDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01);
        public string? HtmlAttentdance { get; set; }  // html lịch làm việc

        public List<NotificationModel>? ListNotification { get; set; }
        public IGrid? GridNotification { get; set; }
        private DotNetObjectReference<HomeController>? dotNetObjectReference { get; set; }
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Trang chủ")
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    dotNetObjectReference = DotNetObjectReference.Create(this);
                    await _jsRuntime.InvokeVoidAsync("HomeController.Init", dotNetObjectReference);
                    await getAttendanceList();
                    await getNotifications();
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

        #region Protected Functions

        /// <summary>
        /// lấy danh sách làm việc của nhân viên trong một tháng
        /// </summary>
        /// <returns></returns>
        private async Task getAttendanceList()
        {
            try
            {
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_WORK_SHEDULE;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.employeeId = EmployeeId;
                request.opt = StartDate.Year.ToString(); // năm
                request.opt1 = StartDate.Month.ToString(); // tháng
                var lstWorSchedule = await _workforceService.GetMasterDataAsync<TimesheetModel>(request, isShowToast: true);
                if (lstWorSchedule.IsNullOrEmpty()) return;
                int daysInMonth = new DateTime(StartDate.Year, StartDate.Month, 1).AddMonths(1).AddDays(-1).Day;
                int firstDay = (int)new DateTime(StartDate.Year, StartDate.Month, 1).DayOfWeek;
                int date = 1;
                StringBuilder calendarHtml = new StringBuilder();
                // tối đa 6 hàng
                for (int i = 0; i < 6; i++)
                {
                    calendarHtml.Append("<tr>");
                    for (int j = 0; j < 7; j++)
                    {
                        if (i == 0 && j < firstDay)
                        {
                            // Các ô trống trước ngày đầu tiên của tháng
                            calendarHtml.Append("<td class=\"bg-light\">");
                            calendarHtml.Append("</td>");
                        }
                        else if (date > daysInMonth)
                        {
                            // Các ô trống sau ngày cuối cùng của tháng
                            calendarHtml.Append("<td class=\"bg-light\">");
                            calendarHtml.Append("</td>");
                        }
                        else
                        {
                            
                            foreach (var item in lstWorSchedule!)
                            {
                                if (item.workingDate.Day == date)
                                {
                                    calendarHtml.Append($"<td class=\"cursor-pointer\" style=\"background-color: {item.bgColorOfWeekdayDayOff} \" onclick=\"HomeController.Render(" + date + ", " + StartDate.Month + ", " + StartDate.Year + ")\">");
                                    calendarHtml.Append($"<span >{(MarkupString)$"{item.workingDate.ToString(GlobalContants.FORMAT_DAY)}"}<span>");
                                    calendarHtml.Append($"<br />");
                                    calendarHtml.Append($"<span>{(MarkupString)$"{item.shiftCode}"}<span>");
                                    calendarHtml.Append($"<br />");
                                    calendarHtml.Append($"<span>{(MarkupString)$"{item.symbolOfWeekdayDayOff}"}<span>");
                                    calendarHtml.Append($"<br />");
                                    calendarHtml.Append($"<span>{(MarkupString)$"Giờ vào: {item.startDateCurrent?.ToString(GlobalContants.FORMAT_TIME)}"}<span>");
                                    calendarHtml.Append($"<br />");
                                    calendarHtml.Append($"<span>{(MarkupString)$"Giờ ra: {item.endDateCurrent?.ToString(GlobalContants.FORMAT_TIME)}"}<span>");
                                    calendarHtml.Append("</td>");
                                    date++;
                                    break;
                                }
                            }
                        }
                    }
                    calendarHtml.Append("</tr>");
                    // Ngừng thêm hàng nếu đã điền đủ ngày trong tháng
                    if (date > daysInMonth) break;
                }
                HtmlAttentdance = calendarHtml.ToString();
            }
            catch { }
        }

        private async Task getNotifications()
        {
            try
            {
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.token = Token;
                request.branchId = BranchId;
                request.type = ProcessConstants.GET_COMBO_TYPE_NOTIFICATION_BY_EMPLOYEE;
                request.opt = EmployeeId.ToString();
                var result = await _masterDataService.GetMasterDataAsync<NotificationModel>(request);
                if (!result.IsNullOrEmpty())
                {
                    ListNotification = result!;
                    StateHasChanged();
                }
            }
            catch { }
        }

        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getAttendanceList();
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
        #endregion
    }
}
