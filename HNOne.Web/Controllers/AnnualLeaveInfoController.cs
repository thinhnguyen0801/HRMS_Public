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
    public class AnnualLeaveInfoController : DocumentControllerBase
    {
        [Inject] IPersonnelService _personnelService { get; init; }
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IWorkforceService _workforceService { get; init; }
        #region Properties
        public int ActiveTabIndex { get; set; } = 0;
        public SearchModel SearchUpdate { get; set; } = new SearchModel();
        public List<AnnualLeaveInfoModel>? ListAnnualLeaveInfo { get; set; }
        public IGrid? GridAnnualLeaveInfo { get; set; }

        public List<ComboboxModel>? ListCboYear { get; set; }
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public List<ComboboxModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public IEnumerable<ComboboxModel>? ListCboStatusSelected { get; set; }
        public List<EmployeeModel>? ListEmpSearch { get; set; } // danh sách nhân viên
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public IEnumerable<ComboboxModel>? ListDepartmentSelected { get; set; }
        public IGrid? GridEmpSearch { get; set; }
        public IReadOnlyList<object>? SelectedDataEmployees { get; set; }
        public bool IsShowFilter { get; set; } = true; // mở rộng vùng tìm kiếm

        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public bool IsShowDialogEmpSearch { get; set; }
        public string? StatusIds { get; set; } // Tình trạng nào
        public string? DepartmentIds { get; set; }
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn

        
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
                        new BreadcrumbModel("Công - phép"),
                        new BreadcrumbModel("Tình công", isActive: true),
                        new BreadcrumbModel("Thông tin phép năm", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    //
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
                    //await _progressService!.Done();
                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        #region Private Functions
        private void initDataAsync()
        {
            SearchUpdate.year = DateTime.Now.Year;
            SearchUpdate.branchId = BranchId;
            int defaultYear = 2025;
            ListCboYear = new List<ComboboxModel>();
            for (int i = defaultYear; i < DateTime.Now.AddYears(1).Year; i++)
            {
                ListCboYear.Add(new ComboboxModel() { id = i, name = $"Năm {i}" });
            }
        }

        private async Task buildComboAsync()
        {
            try
            {
                var getTask1 = _masterDataService.GetDepartmentAsync(UserId, Token, BranchId, opt: CommonConstants.ENUM_ACTIVE); // ds phòng ban
                var getTask2 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiPhatSinhCong)); // ds trạng thái cho phép phát sinh công
                var getTask3 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiNhanVien)); // ds trạng thái
                var getTask4 = _masterDataService.GetBranchAsync(UserId, Token, BranchId, $"{BranchIds}", supperAdmin: IsAdmin ? "Y" : "N");
                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask3,
                    getTask4
                );
                ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, code = m.code, name = m.name })?.ToList();
                ListCboStatus = (await getTask3)?.Where(m => m.rowOrder != 0).Select(m => new ComboboxModel() { code = m.code, name = m.name })?.ToList();
                ListCboBranch = (await getTask4)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
                // gán dữ liệu mặc định
                string[]? statusIds = $"{(await getTask2)?.FirstOrDefault()?.value}".Split(",");
                if (!statusIds.IsNullOrEmpty()
                    && !ListCboStatus.IsNullOrEmpty())
                {
                    ListCboStatusSelected = ListCboStatus!.Where(m => statusIds.Contains(m.code));
                }
            }
            catch (Exception) { throw; }
        }

        private async Task getAnnualLeaveAsync()
        {
            RequestModel request = new RequestModel();
            request.process = ProcessConstants.GET_ANNUAL_LEAVE_INFO;
            request.userId = UserId;
            request.branchId = BranchId;
            request.token = Token;
            request.opt = SearchUpdate.year.ToString();
            request.departmentIds = ListDepartmentSelected.IsNullOrEmpty() ? "" : string.Join(",", ListDepartmentSelected!.Select(m => m.id));
            request.opt1 = ""; // phòng ban
            request.opt2 = ""; // nhân viên
            request.opt3 = SelectedDataEmployees.IsNullOrEmpty() ? "" : string.Join(",", SelectedDataEmployees!.Cast<EmployeeModel>().Select(m => m.id));
            var result = await _workforceService.GetMasterDataAsync<AnnualLeaveInfoModel>(request, isShowToast: true);
            ListAnnualLeaveInfo = result;
        }
        #endregion

        #region Protected Functions

        /// <summary>
        /// lấy danh sách thông tin quản lý phép năm
        /// </summary>
        /// <returns></returns>
        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getAnnualLeaveAsync();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ReLoadDataHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// làm mới danh sách nhân viên
        /// </summary>
        /// <returns></returns>
        protected async Task ReloadEmployeeHandler()
        {
            try
            {
                await ShowLoading(true);
                await Task.Yield();
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.branchId = BranchId;
                request.employeeId = -1;
                request.departmentIds = ListDepartmentSelected.IsNullOrEmpty() ? "" : string.Join(",", ListDepartmentSelected!.Select(m => m.id));
                request.opt = ListCboStatusSelected.IsNullOrEmpty() ? "" : string.Join(",", ListCboStatusSelected!.Select(m => m.code));
                SelectedDataEmployees = null;
                ListEmpSearch = new List<EmployeeModel>();
                ListEmpSearch = await _personnelService.GetEmployeeAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReloadEmployeeHandler");
                _toastService.ShowError(ex.Message);
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
