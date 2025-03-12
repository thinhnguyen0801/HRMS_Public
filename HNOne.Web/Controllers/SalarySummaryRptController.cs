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

namespace HNOne.Web.Controllers
{
    public class SalarySummaryRptController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IReportService _reportService { get; init; }
        public W1Confirm confirm { get; set; }
        #region Properties
        public SearchModel SearchUpdate { get; set; } = new SearchModel();
        public List<SalaryReportModel>? ListSalary { get; set; }
        public PayrollModel PayrollDetail { get; set; } = new PayrollModel(); // chi tiết
        public IGrid? GridSalary { get; set; }
        public IReadOnlyList<object>? ListCboStatusSelected { get; set; } // cbo ds phòng ban
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public IReadOnlyList<object>? ListCboBranchSelected { get; set; } // chi nhánh được chọn
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public IReadOnlyList<object>? ListCboDepartmentSelected { get; set; }
        public bool IsShowFilter { get; set; } = true; // mở rộng vùng tìm kiếm
        public List<ComboboxModel>? ListCboYear { get; set; }
        public List<ComboboxModel>? ListCboMonth { get; set; }

        public int ActiveTabIndex { get; set; } = 0;
        public bool IsShowDetail { get; set; } // show popup chi tiết ngày công
        public string HeaderTextDetail = "";
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-xac-nhan-gio-cong");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Báo cáo", isActive: true),
                        new BreadcrumbModel("Báo cáo tổng hợp lương", isActive: true)
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
            SearchUpdate.year = DateTime.Now.Year;
            SearchUpdate.month = DateTime.Now.Month;
            SearchUpdate.branchId = BranchId;
            int defaultYear = 2024;
            ListCboYear = new List<ComboboxModel>();
            for (int i = defaultYear; i < DateTime.Now.AddYears(2).Year; i++)
            {
                ListCboYear.Add(new ComboboxModel() { id = i, name = $"Năm {i}" });
            }

            ListCboMonth = new List<ComboboxModel>();
            for (int i = 1; i < 13; i++)
            {
                ListCboMonth.Add(new ComboboxModel() { id = i, name = $"Tháng {i}" });
            }
        }
        private async Task buildComboAsync()
        {
            try
            {
                var getTask1 = _masterDataService.GetDepartmentAsync(UserId, Token, BranchId, branchIds: $"{BranchId}", departmentIds: $"{DepartmentIds}", opt: CommonConstants.ENUM_FILTER); // ds phòng ban
                var getTask4 = _masterDataService.GetBranchAsync(UserId, Token, BranchId, $"{BranchIds}", supperAdmin: IsAdmin ? "Y" : "N");
                await Task.WhenAll(
                    getTask1,
                    getTask4
                );
                ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, code = m.code, name = m.name })?.ToList();
                ListCboBranch = (await getTask4)?.Select(m => new ComboboxModel() { id = m.branchId, code = m.branchCode, name = m.branchName })?.ToList();
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
        private async Task getDataReport()
        {
            try
            {
                ListSalary = new List<SalaryReportModel>();
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_RPT_PAYROLL_SUMMARY;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.type = ProcessConstants.GET_ITEM_HEADER;
                request.year = SearchUpdate.year; // năm
                request.month = SearchUpdate.month; // tháng
                request.branchIds = $"{SearchUpdate.branchId}";
                request.departmentIds = ListCboDepartmentSelected.IsNullOrEmpty() ?
                    ListCboDepartment.IsNullOrEmpty() ? DepartmentIds : string.Join(",", ListCboDepartment!.Select(m => m.id))
                    : string.Join(",", ListCboDepartmentSelected!.Cast<ComboboxModel>().Select(m => m.id));
                request.opt1 = ""; // tình trạng
                request.opt2 = ""; // nhân viên
                var response = await _reportService.GetMasterDataAsync<SalaryReportModel>(request, isShowToast: true);
                ListSalary = response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "getDataReport");
                throw ex;
            }
        }
        #endregion

        #region Protected Functions
        protected async Task ComboboxItemChangedHandler(object? value, string controlID = nameof(SearchUpdate.branchId))
        {
            try
            {
                switch (controlID)
                {
                    case nameof(SearchUpdate.branchId):
                        int.TryParse(((ComboboxModel?)value)?.id.ToString(), out int oBranchId);
                        await ShowLoading();
                        ListCboDepartment = new List<ComboboxModel>();
                        var lstDepartment = await _masterDataService.GetDepartmentAsync(UserId, Token, BranchId, branchIds: $"{oBranchId}"
                            , departmentIds: $"{DepartmentIds}", opt: CommonConstants.ENUM_FILTER); // ds phòng ban
                        ListCboDepartment = lstDepartment?.Select(m => new ComboboxModel() { id = m.id, code = m.code, name = m.name })?.ToList(); ;
                        await Task.Delay(100);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "ComboboxItemChangedHandler");
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getDataReport();
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

        /// <summary>
        /// mở chi tiết xem công với các khoản lương
        /// </summary>
        /// <param name="itemSelected"></param>
        /// <returns></returns>
        protected async Task OpenPopupDetailHandler(SalaryReportModel? itemSelected)
        {
            try
            {
                PayrollDetail = new PayrollModel();
                if (itemSelected == null) return;
                await ShowLoading();
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_RPT_PAYROLL_SUMMARY;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.type = ProcessConstants.GET_ITEM_DETAIL;
                request.documentId = itemSelected.id;
                var response = await _reportService.GetMasterDataAsync<PayrollModel>(request, isShowToast: true);
                PayrollDetail = response![0];
                HeaderTextDetail = $"Thông tin lương công chi tiết của nhân viên {itemSelected.employeeCode}";
                IsShowDetail = true;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "RowDoubleClickHandler");
            }
            finally
            {
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
                if (GridSalary == null || ListSalary.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                await ShowLoading();
                await GridSalary!.ExportToXlsxAsync("Bao-cao-tong-hop-luong", new GridXlExportOptions()
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

        /// <summary>
        /// Mở rộng & thu gọn vùng tìm kiếm
        /// </summary>
        protected void ShowFilterHandler() => IsShowFilter = !IsShowFilter;
        #endregion
    }
}
