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
    public class WorkCalculationController : DocumentControllerBase
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
        public List<ShiftAssignmentModel>? ListTimesheetDetail { get; set; }
        public IGrid? GridTimesheetDetail { get; set; }
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
                        new BreadcrumbModel("Công - phép"),
                        new BreadcrumbModel("Tính công", isActive: true),
                        new BreadcrumbModel("Tính công nhân viên", isActive: true)
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
        
        private async Task getDataAttendanceList()
        {
            SelectedItems = null;
            MaxDaysInMonth = DateTime.DaysInMonth(SearchUpdate.year, SearchUpdate.month);
            RequestModel request = new RequestModel();
            request.process = ProcessConstants.GET_WORK_CALCULATE;
            request.userId = UserId;
            request.branchId = BranchId;
            request.token = Token;
            request.opt = SearchUpdate.year.ToString(); // năm
            request.opt1 = SearchUpdate.month.ToString(); // tháng
            request.opt2 = ListDepartmentSelected.IsNullOrEmpty() ? "" : string.Join(",", ListDepartmentSelected!.Select(m => m.id));
            request.opt3 = "";
            request.opt4 = ListCboStatusSelected.IsNullOrEmpty() ? "" : string.Join(",", ListCboStatusSelected!.Select(m => m.code));
            var response = await _workforceService.GetMasterDataAsync<ShiftAssignmentModel>(request, isShowToast: true);
            ListTimesheet = response;
        }
        #endregion

        #region Protected Functions

        protected async Task RowDoubleClickHandler(GridRowClickEventArgs args)
        {
            try
            {
                if (args == null || args.Grid.SelectedDataItem == null) return;
                await ShowLoading();
                ShiftAssignmentModel itemSelected = (ShiftAssignmentModel)args.Grid.SelectedDataItem;
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_WORK_CALCULATE;
                request.type = ProcessConstants.GET_ITEM_DETAIL;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.opt = itemSelected.year.ToString(); // năm
                request.opt1 = itemSelected.month.ToString(); // tháng
                request.opt2 = "";
                request.opt3 = $"{itemSelected.employeeId}";
                request.opt4 = ListCboStatusSelected.IsNullOrEmpty() ? "" : string.Join(",", ListCboStatusSelected!.Select(m => m.code));
                var response = await _workforceService.GetMasterDataAsync<ShiftAssignmentModel>(request, isShowToast: true);
                if(!response.IsNullOrEmpty())
                {
                    HeaderTextDetail = $"Thông tin công chi tiết của nhân viên {itemSelected.employeeCode}";
                    ListTimesheetDetail = response;
                    IsShowDetail = true;
                }    
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
                await getDataAttendanceList();
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


        protected async Task SaveDataHandler()
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
                errorMessage = $"Bạn có chắc muốn lưu kỳ công tháng {SearchUpdate.month} năm {SearchUpdate.year} của nhân viên đang chọn không?";
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.POST_ATTENDENCE_SUMMARY;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.json = JsonConvert.SerializeObject(SelectedItems);
                request.type = "N";
                isConfirm = await _workforceService.UpdateMasterDataAsync(request);
                if (isConfirm)
                {
                    await getDataAttendanceList();
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
        /// chốt kỳ công tháng
        /// </summary>
        /// <returns></returns>
        protected async Task SummitWork()
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
                errorMessage = $"Bạn có chắc muốn chốt kỳ công tháng {SearchUpdate.month} năm {SearchUpdate.year} của nhân viên đang chọn không?";
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.POST_ATTENDENCE_SUMMARY;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.json = JsonConvert.SerializeObject(SelectedItems);
                request.type = "L";
                isConfirm = await _workforceService.UpdateMasterDataAsync(request);
                if (isConfirm)
                {
                    await getDataAttendanceList();
                }
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
        
        protected void GridTimesheetDetailCustomizeElement(GridCustomizeElementEventArgs e)
        {
            try
            {
                if (e.ElementType == GridElementType.DataRow && GridTimesheetDetail != null)
                {
                    var employee = (ShiftAssignmentModel)GridTimesheetDetail.GetDataItem(e.VisibleIndex);
                    if (!string.IsNullOrEmpty(employee?.bgColor))
                    {
                        e.Style = $"background-color: {employee.bgColor}";
                    }
                }
            }
            catch (Exception ex) { }
        }
        #endregion
    }
}
