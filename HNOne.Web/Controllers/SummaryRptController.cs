using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model.Models;
using HNOne.Model;
using HNOne.Web.Commons;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Data;
using HNOne.Web.Components.Controls;
using Newtonsoft.Json;
using HNOne.Web.Services;
using System.Diagnostics;
using Microsoft.JSInterop;

namespace HNOne.Web.Controllers
{
    public class SummaryRptController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IReportService _reportService { get; init; }
        [Inject] IConfiguration _configuration { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        #region Properties
        public SummaryReportModel SummaryReport { get; set; } = new SummaryReportModel();
        public List<EmployeeModel>? ListBirthdays { get; set; } // danh sách sinh nhật sắp đến hạn
        public List<EmployeeModel>? ListUpcomingWorkAnniversaries { get; set; } // danh sách kỷ niệm ngày vào làm sắp đến hạn
        public List<EmployeeModel>? ListEmployeesEndingProbationSoon { get; set; } // danh sách nhân viên sắp hết hạn thử việc
        public List<EmployeeModel>? ListEmployeesOnMaternityLeaveSoon { get; set; } // danh sách nhân viên nghỉ thai sản sắp đến hạn
        public List<ComboboxModel> ListCboFilterDays { get; set; } = new List<ComboboxModel>(); // cbo ds lọc ngày
        public string? PopupTypeName { get; set; }// gán tên cho popup
        public List<EmployeeModel>? ListEmployeeByOverview { get; set; } // lấy nhân viên hiển thị ở popup
        public List<EmployeeSalaryHistoryModel>? ListContracts { get; set; } // lấy nhân viên hiển thị ở popup
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    string errMessage = await CheckMenuPermissionAsync("tong-quan");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Tổng quan", isActive: true),
                        new BreadcrumbModel("Tổng quan nhân sự", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    initDataAsync();
                    await buildComboAsync();
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
        private void initDataAsync()
        {
            var lstDaysDefault = new List<ComboboxModel>()
                {
                    new ComboboxModel() {code = "7", name = "7 Ngày"},
                    new ComboboxModel() {code = "15", name = "15 Ngày"},
                    new ComboboxModel() {code = "30", name = "30 Ngày"}
                };
            ListCboFilterDays = lstDaysDefault;
            SummaryReport.dayEmployeesEndingProbationSoon = "7";
            SummaryReport.dayEmployeesOnMaternityLeaveSoon = "30";
            SummaryReport.dayBirthdays = "7";
            SummaryReport.dayUpcomingWorkAnniversaries = "30";
            SummaryReport.dayContractSoon = "30";
        }
        private async Task buildComboAsync()
        {
            try
            {
                
                var task0 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.BoLocNgayBaoCao)); // ds trạng thái
                var task1 = getDataReport(ProcessConstants.GET_REPORT_SUMMARY_EMPLOYEE);
                var task2 = getDataReport(ProcessConstants.GET_REPORT_SUMMARY_STATISTICS_BY_DAY);

                var task3 = getDataReport(ProcessConstants.GET_REPORT_SUMMARY_BIRTHDAY, numOfDay: SummaryReport.dayBirthdays);
                var task4 = getDataReport(ProcessConstants.GET_REPORT_SUMMARY_UPCOMING_WORK_ANNIVERSARIES, numOfDay: SummaryReport.dayUpcomingWorkAnniversaries);
                var task5 = getDataReport(ProcessConstants.GET_REPORT_SUMMARY_EMPLOYEES_ENDING_PROBATIONSOON, numOfDay: SummaryReport.dayEmployeesEndingProbationSoon);
                var task6 = getDataReport(ProcessConstants.GET_REPORT_SUMMARY_EMPLOYEES_ONMATERNITY_LEAVESOON, numOfDay: SummaryReport.dayEmployeesOnMaternityLeaveSoon);
                var task7 = getDataReport(ProcessConstants.GET_REPORT_SUMMARY_CONTRACT_SOON, numOfDay: SummaryReport.dayContractSoon);
                var task8 = getDataReport(ProcessConstants.GET_REPORT_SUMMARY_CONTRACT_EXPIRED, numOfDay: SummaryReport.dayContractSoon);
                await Task.WhenAll(
                    task0,
                    task1,
                    task2,
                    task3,
                    task4,
                    task5,
                    task6,
                    task7,
                    task8
                );

                var lstDays = (await task0);
                if(!lstDays.IsNullOrEmpty())
                {
                    ListCboFilterDays = lstDays!.Select(m => new ComboboxModel() { code = m.code, name = m.name }).ToList();
                }    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "buildComboAsync");
                throw ex;
            }
        }

        /// <summary>
        /// lấy dữ liệu báo cáo
        /// </summary>
        /// <returns></returns>
        private async Task getDataReport(string reportType, string? numOfDay = "7")
        {
            try
            {
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_RPT_SUMMARY;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.type = reportType;
                request.opt = $"{numOfDay}"; // tình trạng
                switch (reportType)
                {
                    case ProcessConstants.GET_REPORT_SUMMARY_EMPLOYEE:
                        var summary1 = (await _reportService.GetMasterDataAsync<SummaryReportModel>(request, isShowToast: false))?.FirstOrDefault() ?? new SummaryReportModel();
                        SummaryReport.totalEmployees = summary1.totalEmployees;
                        SummaryReport.totalOfficialEmployees = summary1.totalOfficialEmployees;
                        SummaryReport.totalProbationaryEmployees = summary1.totalProbationaryEmployees;
                        SummaryReport.totalEmployeesOnMaternityLeave = summary1.totalEmployeesOnMaternityLeave;
                        SummaryReport.totalOtherEmployees = summary1.totalOtherEmployees;
                        break;
                    case ProcessConstants.GET_REPORT_SUMMARY_STATISTICS_BY_DAY:
                        var summary2 = (await _reportService.GetMasterDataAsync<SummaryReportModel>(request, isShowToast: false))?.FirstOrDefault() ?? new SummaryReportModel();
                        SummaryReport.totalLeaveRequests = summary2.totalLeaveRequests;
                        SummaryReport.totalOvertimes = summary2.totalOvertimes;
                        SummaryReport.totalLateArrivalsAndEarlyLeaves = summary2.totalLateArrivalsAndEarlyLeaves;
                        SummaryReport.totalMissingTimeAttendance = summary2.totalMissingTimeAttendance;
                        break;
                    
                    case ProcessConstants.GET_REPORT_SUMMARY_BIRTHDAY:
                        ListBirthdays = new List<EmployeeModel>();
                        var summary3 = (await _reportService.GetMasterDataAsync<EmployeeModel>(request, isShowToast: false));
                        summary3 = summary3?.Update(m =>
                        {
                            Dictionary<string, string> pParams = new Dictionary<string, string>
                            {
                                { "pActionType", nameof(EnumType.Update) },
                                { "pDocEntry", $"{m.id}" },
                            };
                            m.link = "ho-so-nhan-vien?key=" + _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
                            if (!string.IsNullOrEmpty(m.imageUrl))
                            {
                                string apiUrl = _configuration.GetSection("appSettings:ImageUrl").Value + "";
                                m.imageViewUrl = $"{apiUrl}{nameof(EmployeeController)}/{m.imageUrl}";
                            }
                        })?.ToList();
                        ListBirthdays = summary3 ?? new List<EmployeeModel>();
                        SummaryReport.totalBirthdays = ListBirthdays.Count();
                        break;
                    case ProcessConstants.GET_REPORT_SUMMARY_UPCOMING_WORK_ANNIVERSARIES:
                        ListUpcomingWorkAnniversaries = new List<EmployeeModel>();
                        var summary4 = (await _reportService.GetMasterDataAsync<EmployeeModel>(request, isShowToast: false));
                        summary4 = summary4?.Update(m =>
                        {
                            Dictionary<string, string> pParams = new Dictionary<string, string>
                            {
                                { "pActionType", nameof(EnumType.Update) },
                                { "pDocEntry", $"{m.id}" },
                            };
                            m.link = "ho-so-nhan-vien?key=" + _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
                            if (!string.IsNullOrEmpty(m.imageUrl))
                            {
                                string apiUrl = _configuration.GetSection("appSettings:ImageUrl").Value + "";
                                m.imageViewUrl = $"{apiUrl}{nameof(EmployeeController)}/{m.imageUrl}";
                            }
                        })?.ToList();
                        ListUpcomingWorkAnniversaries = summary4 ?? new List<EmployeeModel>();
                        SummaryReport.totalUpcomingWorkAnniversaries = ListUpcomingWorkAnniversaries.Count();
                        break;
                    case ProcessConstants.GET_REPORT_SUMMARY_EMPLOYEES_ENDING_PROBATIONSOON:
                        ListEmployeesEndingProbationSoon = new List<EmployeeModel>();
                        var summary5 = (await _reportService.GetMasterDataAsync<EmployeeModel>(request, isShowToast: false));
                        summary5 = summary5?.Update(m =>
                        {
                            Dictionary<string, string> pParams = new Dictionary<string, string>
                            {
                                { "pActionType", nameof(EnumType.Update) },
                                { "pDocEntry", $"{m.id}" },
                            };
                            m.link = "ho-so-nhan-vien?key=" + _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
                            if (!string.IsNullOrEmpty(m.imageUrl))
                            {
                                string apiUrl = _configuration.GetSection("appSettings:ImageUrl").Value + "";
                                m.imageViewUrl = $"{apiUrl}{nameof(EmployeeController)}/{m.imageUrl}";
                            }
                        })?.OrderBy(m => m.probationEndDate)?.ToList();
                        ListEmployeesEndingProbationSoon = summary5 ?? new List<EmployeeModel>();
                        SummaryReport.totalEmployeesEndingProbationSoon = ListEmployeesEndingProbationSoon.Count();
                        break;
                    case ProcessConstants.GET_REPORT_SUMMARY_EMPLOYEES_ONMATERNITY_LEAVESOON:
                        ListEmployeesOnMaternityLeaveSoon = new List<EmployeeModel>();
                        var summary6 = (await _reportService.GetMasterDataAsync<EmployeeModel>(request, isShowToast: false));
                        summary6 = summary6?.Update(m =>
                        {
                            Dictionary<string, string> pParams = new Dictionary<string, string>
                            {
                                { "pActionType", nameof(EnumType.Update) },
                                { "pDocEntry", $"{m.id}" },
                            };
                            m.link = "ho-so-nhan-vien?key=" + _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
                            if (!string.IsNullOrEmpty(m.imageUrl))
                            {
                                string apiUrl = _configuration.GetSection("appSettings:ImageUrl").Value + "";
                                m.imageViewUrl = $"{apiUrl}{nameof(EmployeeController)}/{m.imageUrl}";
                            }
                        })?.OrderBy(m=> m.probationEndDate)?.ToList();
                        ListEmployeesOnMaternityLeaveSoon = summary6 ?? new List<EmployeeModel>();
                        SummaryReport.totalEmployeesOnMaternityLeaveSoon = ListEmployeesOnMaternityLeaveSoon.Count();
                        break;
                    case ProcessConstants.GET_REPORT_SUMMARY_CONTRACT_SOON:
                        var summary7 = (await _reportService.GetMasterDataAsync<SummaryReportModel>(request, isShowToast: false))?.FirstOrDefault() ?? new SummaryReportModel();
                        SummaryReport.totalContractSoon = summary7.totalContractSoon;
                        break;
                    case ProcessConstants.GET_REPORT_SUMMARY_CONTRACT_EXPIRED:
                        var summary8 = (await _reportService.GetMasterDataAsync<SummaryReportModel>(request, isShowToast: false))?.FirstOrDefault() ?? new SummaryReportModel();
                        SummaryReport.totalContractExpired = summary8.totalContractExpired;
                        break;
                    case ProcessConstants.GET_REPORT_SUMMARY_STATISTICS_BY_DAY_CHECK_IN_OUT_DETAIL:
                    case ProcessConstants.GET_REPORT_SUMMARY_STATISTICS_BY_DAY_LATE_EARLY_DETAIL:
                    case ProcessConstants.GET_REPORT_SUMMARY_STATISTICS_BY_DAY_OVERTIME_DETAIL:
                    case ProcessConstants.GET_REPORT_SUMMARY_STATISTICS_BY_DAY_LEAVE_REQUEST_DETAIL:
                        ListEmployeeByOverview = new List<EmployeeModel>();
                        ListContracts = new List<EmployeeSalaryHistoryModel>();
                        var summary9 = (await _reportService.GetMasterDataAsync<EmployeeModel>(request, isShowToast: false));
                        summary9 = summary9?.Update(m =>
                        {
                            Dictionary<string, string> pParams = new Dictionary<string, string>
                            {
                                { "pActionType", nameof(EnumType.Update) },
                                { "pDocEntry", $"{m.id}" },
                            };
                            m.link = "ho-so-nhan-vien?key=" + _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
                            if (!string.IsNullOrEmpty(m.imageUrl))
                            {
                                string apiUrl = _configuration.GetSection("appSettings:ImageUrl").Value + "";
                                m.imageViewUrl = $"{apiUrl}{nameof(EmployeeController)}/{m.imageUrl}";
                            }
                        })?.OrderBy(m => m.probationEndDate)?.ToList();
                        ListEmployeeByOverview = summary9 ?? new List<EmployeeModel>();
                        break;
                    case ProcessConstants.GET_REPORT_SUMMARY_CONTRACT_SOON_DETAIL:
                    case ProcessConstants.GET_REPORT_SUMMARY_CONTRACT_EXPIRED_DETAIL:
                        ListEmployeeByOverview = new List<EmployeeModel>();
                        ListContracts = new List<EmployeeSalaryHistoryModel>();
                        var summary10 = (await _reportService.GetMasterDataAsync<EmployeeSalaryHistoryModel>(request, isShowToast: false));
                        summary10 = summary10?.Update(m =>
                        {
                            Dictionary<string, string> pParams = new Dictionary<string, string>
                            {
                                { "pActionType", nameof(EnumType.Update) },
                                { "pDocEntry", $"{m.contractId}" },
                            };
                            m.linkContract = "chi-tiet-hop-dong?key=" + _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
                        })?.OrderBy(m => m.endDate)?.ToList();
                        ListContracts = summary10 ?? new List<EmployeeSalaryHistoryModel>();
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "getDataReport");
                throw ex;
            }
        }

        protected async Task ComboboxValueChangedHandler(object? value, string controlID = ProcessConstants.GET_REPORT_SUMMARY_EMPLOYEES_ENDING_PROBATIONSOON)
        {
            try
            {
                switch (controlID)
                {
                    case ProcessConstants.GET_REPORT_SUMMARY_EMPLOYEES_ENDING_PROBATIONSOON:
                        SummaryReport.dayEmployeesEndingProbationSoon = value?.ToString();
                        await ShowLoading();
                        await getDataReport(controlID, SummaryReport.dayEmployeesEndingProbationSoon);
                        break;
                    case ProcessConstants.GET_REPORT_SUMMARY_BIRTHDAY:
                        SummaryReport.dayBirthdays = value?.ToString();
                        await ShowLoading();
                        await getDataReport(controlID, SummaryReport.dayBirthdays);
                        break;
                    case ProcessConstants.GET_REPORT_SUMMARY_UPCOMING_WORK_ANNIVERSARIES:
                        SummaryReport.dayUpcomingWorkAnniversaries = value?.ToString();
                        await ShowLoading();
                        await getDataReport(controlID, SummaryReport.dayUpcomingWorkAnniversaries);
                        break;
                    case ProcessConstants.GET_REPORT_SUMMARY_EMPLOYEES_ONMATERNITY_LEAVESOON:
                        SummaryReport.dayEmployeesOnMaternityLeaveSoon = value?.ToString();
                        await ShowLoading();
                        await getDataReport(controlID, SummaryReport.dayEmployeesOnMaternityLeaveSoon);
                        break;
                    case ProcessConstants.GET_REPORT_SUMMARY_CONTRACT_SOON:
                        SummaryReport.dayContractSoon = value?.ToString();
                        await ShowLoading();
                        await getDataReport(controlID, SummaryReport.dayContractSoon);
                        break;
                    default:
                        break;
                }    
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "ComboboxValueChangedHandler");
            }
            finally
            {
                await Task.Delay(200);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// Mở popup danh sách
        /// </summary>
        /// <param name="popupType"></param>
        protected async void OpenPopupHandler(string popupType = nameof(SummaryReport.totalLeaveRequests))
        {
            try
            {
                switch (popupType)
                {
                    case nameof(SummaryReport.totalLeaveRequests):
                        if (SummaryReport.totalLeaveRequests < 1) return;
                        await ShowLoading();
                        PopupTypeName = "Nhân viên xin nghỉ";
                        await getDataReport(ProcessConstants.GET_REPORT_SUMMARY_STATISTICS_BY_DAY_LEAVE_REQUEST_DETAIL);
                        break;
                    case nameof(SummaryReport.totalOvertimes):
                        if (SummaryReport.totalOvertimes < 1) return;
                        await ShowLoading();
                        PopupTypeName = "Nhân viên tăng ca";
                        await getDataReport(ProcessConstants.GET_REPORT_SUMMARY_STATISTICS_BY_DAY_OVERTIME_DETAIL);
                        break;
                    case nameof(SummaryReport.totalLateArrivalsAndEarlyLeaves):
                        if (SummaryReport.totalLateArrivalsAndEarlyLeaves < 1) return;
                        await ShowLoading();
                        PopupTypeName = "Nhân viên đi trễ/về sớm";
                        await getDataReport(ProcessConstants.GET_REPORT_SUMMARY_STATISTICS_BY_DAY_LATE_EARLY_DETAIL);
                        break;
                    case nameof(SummaryReport.totalMissingTimeAttendance):
                        if (SummaryReport.totalMissingTimeAttendance < 1) return;
                        await ShowLoading();
                        PopupTypeName = "Nhân viên quên chấm công";
                        await getDataReport(ProcessConstants.GET_REPORT_SUMMARY_STATISTICS_BY_DAY_CHECK_IN_OUT_DETAIL);
                        break;
                    case nameof(SummaryReport.totalContractSoon):
                        if (SummaryReport.totalContractSoon < 1) return;
                        await ShowLoading();
                        PopupTypeName = "Hợp đồng sắp đến hạn";
                        await getDataReport(ProcessConstants.GET_REPORT_SUMMARY_CONTRACT_SOON_DETAIL);
                        break;
                    case nameof(SummaryReport.totalContractExpired):
                        if (SummaryReport.totalContractExpired < 1) return;
                        await ShowLoading();
                        PopupTypeName = "Hợp đồng hết hạn";
                        await getDataReport(ProcessConstants.GET_REPORT_SUMMARY_CONTRACT_EXPIRED_DETAIL);
                        break;
                    default: return;
                }
                await _jsRuntime.InvokeVoidAsync("toggleOffcanvas", "offcanvasBottom");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenPopupHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(100);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion
    }
}
