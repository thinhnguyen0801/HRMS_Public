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
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IWorkforceService _workforceService { get; init; }
        #region Properties
        public int ActiveTabIndex { get; set; } = 0;
        public SearchModel SearchUpdate { get; set; } = new SearchModel();
        public List<AnnualLeaveInfoModel>? ListAnnualLeaveInfo { get; set; }
        public IGrid? GridAnnualLeaveInfo { get; set; }
        public List<ComboboxModel>? ListCboYear { get; set; }
        public List<ComboboxModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public IEnumerable<ComboboxModel>? ListCboStatusSelected { get; set; }
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public IEnumerable<ComboboxModel>? ListDepartmentSelected { get; set; }

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
            int defaultYear = 2024;
            ListCboYear = new List<ComboboxModel>();
            for (int i = defaultYear; i < DateTime.Now.AddYears(2).Year; i++)
            {
                ListCboYear.Add(new ComboboxModel() { id = i, name = $"Năm {i}" });
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
                ListCboStatus = (await getTask4)?.Select(m => new ComboboxModel() {code = m.code, name = m.name })?.ToList();
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

        /// <summary>
        /// lấy danh sách thông tin quản lý phép năm
        /// </summary>
        /// <returns></returns>
        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_ANNUAL_LEAVE_INFO;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.opt = SearchUpdate.year.ToString();
                request.opt1 = ""; // phòng ban
                request.opt2 = ""; // nhân viên
                request.opt3 = ""; // tình trạng
                var result = await _workforceService.GetMasterDataAsync<AnnualLeaveInfoModel>(request, isShowToast: true);
                ListAnnualLeaveInfo = result;
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
        #endregion
    }
}
