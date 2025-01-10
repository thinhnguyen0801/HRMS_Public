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
    public class MonthlySalaryCalculationController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] ISalaryService _salaryService { get; init; }
        public W1Confirm confirm { get; set; }
        const string STRING_KEY_EVENT_POST = "WORK_CALCULATION_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "LEAVE_REQUEST_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_PUT_ACCEPT = "WORK_CALCULATION_CONTROLLER_PUT_ACCEPT";
        #region Properties
        public int ActiveTabIndex { get; set; } = 0;
        public SearchModel SearchUpdate { get; set; } = new SearchModel();
        public List<PayrollModel>? ListSalary { get; set; }
        public IGrid? GridSalary { get; set; }
        public IGrid? GridInsuranceSalary { get; set; }
        public IGrid? GridTaxSalary { get; set; }
        public IReadOnlyList<object>? SelectedItems { get; set; } = null;
        public List<ComboboxModel>? ListCboYear { get; set; }
        public List<ComboboxModel>? ListCboMonth { get; set; }
        public List<ComboboxModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public IEnumerable<ComboboxModel>? ListCboStatusSelected { get; set; }
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public IEnumerable<ComboboxModel>? ListDepartmentSelected { get; set; }

        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public bool IsShowDialogEmpSearch { get; set; }
        public string? StatusIds { get; set; } // Tình trạng nào
        public string? DepartmentIds { get; set; }
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn
        public int MaxDaysInMonth { get; set; } = 30; // max số ngày trong tháng
        public bool IsShowDetail { get; set; } // show popup chi tiết ngày công
        public string HeaderTextDetail = "";

        // nút quyền
        public bool IsAllowPost { get; set; }
        public bool IsAllowPut { get; set; }
        public bool IsAllowPutAccept { get; set; }
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
                        new BreadcrumbModel("Tính lương"),
                        new BreadcrumbModel("Tính lương tháng", isActive: true),
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
            SearchUpdate.month = DateTime.Now.Month;
            MaxDaysInMonth = DateTime.DaysInMonth(SearchUpdate.year, SearchUpdate.month);
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
                var getTask1 = _masterDataService.GetDepartmentAsync(UserId, Token, BranchId, opt: CommonConstants.ENUM_ACTIVE); // ds phòng ban
                var getTask4 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiNhanVien)); // ds trạng thái
                await Task.WhenAll(
                    getTask1,
                    getTask4
                );

                ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, code = m.code, name = m.name })?.ToList();
                ListCboStatus = (await getTask4)?.Where(m => m.rowOrder != 0).Select(m => new ComboboxModel() { code = m.code, name = m.name })?.ToList();
            }
            catch (Exception) { throw; }
        }

        private async Task getMonthlySalaryList()
        {
            SelectedItems = null;
            MaxDaysInMonth = DateTime.DaysInMonth(SearchUpdate.year, SearchUpdate.month);
            RequestModel request = new RequestModel();
            request.process = ProcessConstants.GET_MONTHLY_SALARY;
            request.userId = UserId;
            request.branchId = BranchId;
            request.token = Token;
            request.opt = SearchUpdate.year.ToString(); // năm
            request.opt1 = SearchUpdate.month.ToString(); // tháng
            request.opt2 = ListDepartmentSelected.IsNullOrEmpty() ? "" : string.Join(",", ListDepartmentSelected!.Select(m => m.id));
            request.opt3 = "";
            request.opt4 = ListCboStatusSelected.IsNullOrEmpty() ? "" : string.Join(",", ListCboStatusSelected!.Select(m => m.code));
            var response = await _salaryService.GetMasterDataAsync<PayrollModel>(request, isShowToast: true);
            ListSalary = response;
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
            IsAllowPutAccept = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_PUT_ACCEPT) != null;
        }
        #endregion

        #region Protected Functions
        protected async Task OpenPopupHandler(string type = nameof(EmployeeSelected),
            string popupType = nameof(SearchUpdate.employeeCode))
        {
            try
            {
                pPopupType = popupType;
                switch (type)
                {
                    case nameof(EmployeeSelected):
                        //ListCboDepartment ??= new();
                        //DepartmentIds = string.Join(",", ListCboDepartment.Select(m => m.id));
                        IsShowDialogEmpSearch = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "OpenPopupHandler");
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// chọn nhân viên
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected async Task SelectEmployeeHandler()
        {
            try
            {
                if (EmployeeSelected == null)
                {
                    ShowWarning(string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Nhân viên"));
                    return;
                }
                EmployeeModel employee = (EmployeeModel)EmployeeSelected;
                switch (pPopupType)
                {
                    case nameof(SearchUpdate.employeeCode):
                        SearchUpdate.employeeId = employee.id;
                        SearchUpdate.employeeCode = employee.code;
                        SearchUpdate.employeeName = employee.name;
                        IsShowDialogEmpSearch = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "SelectEmployeeHandler");
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// callback nhân viên
        /// </summary>
        /// <param name="lstEmp"></param>
        protected void EventCallbackEmpChangedHandler(object? lstEmp) => EmployeeSelected = lstEmp;

        /// <summary>
        /// lấy danh sách thông tin quản lý phép năm
        /// </summary>
        /// <returns></returns>
        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getMonthlySalaryList();
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
        /// Lưu công tháng
        /// </summary>
        /// <returns></returns>
        protected async Task SaveDataHandler()
        {
            try
            {
                //await checkPermission(MenuId);
                //if (!IsAllowPost)
                //{
                //    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                //    return;
                //}
                if (SelectedItems.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                string errorMessage = string.Empty;
                string fieldName = string.Empty; // trả ra trường nào cần validate
                bool isConfirm = true;
                errorMessage = $"Bạn có chắc muốn lưu kỳ lương tháng {SearchUpdate.month} năm {SearchUpdate.year} của nhân viên đang chọn không?";
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.POST_PAYROLL_SALARY;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.json = JsonConvert.SerializeObject(SelectedItems);
                request.type = "N";
                isConfirm = await _salaryService.UpdateMasterDataAsync(request);
                if (isConfirm)
                {
                    await getMonthlySalaryList();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "SaveDataHandler");
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// Chốt lương nhân viên
        /// </summary>
        /// <returns></returns>
        protected async Task LockPayrollEmployeeHandler()
        {
            try
            {
                //await checkPermission(MenuId);
                //if (!IsAllowPut)
                //{
                //    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                //    return;
                //}
                if (SelectedItems.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                string errorMessage = string.Empty;
                string fieldName = string.Empty; // trả ra trường nào cần validate
                bool isConfirm = true;
                errorMessage = $"Bạn có chắc muốn khóa kỳ lương tháng {SearchUpdate.month} năm {SearchUpdate.year} của nhân viên đang chọn không?";
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.POST_PAYROLL_SALARY;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.json = JsonConvert.SerializeObject(SelectedItems);
                request.type = "L";
                isConfirm = await _salaryService.UpdateMasterDataAsync(request);
                if (isConfirm)
                {
                    await getMonthlySalaryList();
                }
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "LockPayrollEmployeeHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// Mở khóa kỳ lương để chỉnh sửa
        /// </summary>
        /// <returns></returns>
        protected async Task UnlockPayrollEmployeeHandler()
        {
            try
            {
                if (SelectedItems.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                string errorMessage = string.Empty;
                string fieldName = string.Empty; // trả ra trường nào cần validate
                bool isConfirm = true;
                errorMessage = $"Bạn có chắc muốn mở kỳ lương tháng {SearchUpdate.month} năm {SearchUpdate.year} của nhân viên đang chọn không?";
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.PUT_UNLOCK_PAYROLL_SALARY;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.json = JsonConvert.SerializeObject(SelectedItems);
                request.type = "UL";
                isConfirm = await _salaryService.UpdateMasterDataAsync(request);
                if (isConfirm)
                {
                    await getMonthlySalaryList();
                }
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "LockPayrollEmployeeHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// Chốt lương nhân viên
        /// </summary>
        /// <returns></returns>
        protected async Task LockPayrollBranchHandler()
        {
            try
            {
                //await checkPermission(MenuId);
                //if (!IsAllowPut)
                //{
                //    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                //    return;
                //}
                if (SelectedItems.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                string errorMessage = string.Empty;
                string fieldName = string.Empty; // trả ra trường nào cần validate
                bool isConfirm = true;
                errorMessage = $"Bạn có chắc muốn chốt kỳ lương tháng {SearchUpdate.month} năm {SearchUpdate.year} của chi nhánh?";
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.POST_PAYROLL_SALARY;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.json = JsonConvert.SerializeObject(SelectedItems);
                //request.type = "L";
                //isConfirm = await _salaryService.UpdateMasterDataAsync(request);
                //if (isConfirm)
                //{
                //    await getMonthlySalaryList();
                //}
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "LockPayrollEmployeeHandler");
                ShowError(ex.Message);
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
                if (ActiveTabIndex == 0)
                {
                    if (GridSalary == null || ListSalary.IsNullOrEmpty())
                    {
                        ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                        return;
                    }
                    await ShowLoading();
                    await GridSalary!.ExportToXlsxAsync($"Bang-luong-thang-{SearchUpdate.month}-{SearchUpdate.year}", new GridXlExportOptions()
                    {
                        ExportTotalSummaries = true,
                        ExportGroupSummaries = false
                    });
                    return;
                }
                if (ActiveTabIndex == 1)
                {
                    if (GridInsuranceSalary == null || ListSalary.IsNullOrEmpty())
                    {
                        ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                        return;
                    }
                    await ShowLoading();
                    await GridInsuranceSalary!.ExportToXlsxAsync($"Bang-luong-trich-nop-thang-{SearchUpdate.month}-{SearchUpdate.year}", new GridXlExportOptions()
                    {
                        ExportTotalSummaries = true,
                        ExportGroupSummaries = false
                    });
                    return;
                }
                if (GridTaxSalary == null || ListSalary.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                await ShowLoading();
                await GridTaxSalary!.ExportToXlsxAsync($"Bang-luong-TTNCN-thang-{SearchUpdate.month}-{SearchUpdate.year}", new GridXlExportOptions()
                {
                    ExportTotalSummaries = true,
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
