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

namespace HNOne.Web.Controllers
{
    public class MonthlySalaryCalculationController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IWorkforceService _workforceService { get; init; }
        public W1Confirm confirm { get; set; }
        #region Properties
        public int ActiveTabIndex { get; set; } = 0;
        public SearchModel SearchUpdate { get; set; } = new SearchModel();
        public List<ShiftAssignmentModel>? ListTimesheet { get; set; }
        public IGrid? GridTimesheet { get; set; }
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
                var getTask1 = _masterDataService.GetDepartmentAsync(UserId, Token); // ds phòng ban
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
        #endregion
    }
}
