using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using HNOne.Web.Models;
using HNOne.Web.Components.Controls;
using HNOne.Model.Entities;

namespace HNOne.Web.Controllers
{
    public class EmployeeListController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IPersonnelService _personnelService { get; init; }
        public W1Confirm confirm { get; set; }
        const string STRING_KEY_EVENT_POST = "EMPLOYEE_CONTROLLER_POST";
        const string STRING_KEY_EVENT_DELETE = "EMPLOYEE_CONTROLLER_DELETE";
        const string STRING_KEY_EVENT_PUT = "EMPLOYEE_CONTROLLER_PUT";
        #region Properties
        public List<EmployeeModel>? ListEmployee { get; set; }
        public IGrid? GridEmployee { get; set; }
        public List<EmployeeModel>? ListEmployeeOff { get; set; }
        public IGrid? GridEmployeeOff { get; set; }
        public IReadOnlyList<object>? SelectedEmployees { get; set; } = null;
        public IReadOnlyList<object>? SelectedEmployeeOffs { get; set; } = null;
        public List<ComboboxModel>? ListCboStatus { get; set; } // danh sách tình trạng nhân viên
        public IReadOnlyList<object>? ListCboStatusSelected { get; set; } // cbo ds phòng ban
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public IReadOnlyList<object>? ListCboBranchSelected { get; set; } // chi nhánh được chọn
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public IReadOnlyList<object>? ListCboDepartmentSelected { get; set; }
        public bool IsShowFilter { get; set; } = true; // mở rộng vùng tìm kiếm
        public int ActiveTabIndex { get; set; } = 0;
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
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-nhan-vien");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Nhân sự", isActive: true),
                        new BreadcrumbModel("Danh sách nhân viên", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await buildComboAsync();
                    await getEmployee();
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

        /// <summary>
        /// kiểm tra quyền nút
        /// </summary>
        /// <returns></returns>
        private async Task checkPermission(string menuId)
        {
            List<string> lstKey = await CheckEventPermission(menuId);
            IsAllowPost = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_POST) != null;
            IsAllowDelete = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_DELETE) != null;
            IsAllowPut = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_PUT) != null;
        }

        private async Task buildComboAsync()
        {
            try
            {
                var getTask1 = _masterDataService.GetDepartmentAsync(UserId, Token, BranchId, $"{BranchIds}", departmentIds: $"{DepartmentIds}", opt: CommonConstants.ENUM_FILTER); // ds phòng ban
                var getTask3 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiNhanVien)); // ds trạng thái
                var getTask4 = _masterDataService.GetBranchAsync(UserId, Token, BranchId, $"{BranchIds}", supperAdmin: IsAdmin ? "Y" : "N");
                await Task.WhenAll(
                    getTask1,
                    getTask3,
                    getTask4
                );
                ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, code = m.code, name = m.name })?.ToList();
                ListCboStatus = (await getTask3)?.Where(m => m.rowOrder != 0).Select(m => new ComboboxModel() { code = m.code, name = m.name })?.ToList();
                ListCboBranch = (await getTask4)?.Select(m => new ComboboxModel() { id = m.branchId, code = m.branchCode, name = m.branchName })?.ToList();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "buildComboAsync");
            }
        }
        private async Task getEmployee()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.token = Token;
            request.branchId = BranchId;
            request.opt = ListCboStatusSelected.IsNullOrEmpty() ? "" : string.Join(",", ListCboStatusSelected!.Cast<ComboboxModel>().Select(m => m.code)); ;
            request.branchIds = ListCboBranchSelected.IsNullOrEmpty() ? BranchIds : string.Join(",", ListCboBranchSelected!.Cast<ComboboxModel>().Select(m=>m.id));
            request.departmentIds = ListCboDepartmentSelected.IsNullOrEmpty() ? DepartmentIds : string.Join(",", ListCboDepartmentSelected!.Cast<ComboboxModel>().Select(m => m.id));
            request.type = CommonConstants.ENUM_LIST;
            request.opt1 = ActiveTabIndex == 0 ? "N" : "Y"; // lấy nhân viên nghỉ việc
            var lstEmp = await _personnelService.GetEmployeeAsync(request);
            if(IsAllowPut)
            {
                lstEmp = lstEmp?.Update(m =>
                {
                    Dictionary<string, string> pParams = new Dictionary<string, string>
                    {
                        { "pActionType", nameof(EnumType.Update) },
                        { "pDocEntry", $"{m.id}" },
                    };
                    m.link = "ho-so-nhan-vien?key=" + _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
                })?.ToList();
            }
            if (ActiveTabIndex == 0)
            {
                ListEmployee = new List<EmployeeModel>();
                ListEmployee = lstEmp;
                return;
            }
            ListEmployeeOff = new List<EmployeeModel>();
            ListEmployeeOff = lstEmp;
            
        }
        #endregion

        #region Protected Functions
        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getEmployee();
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

        protected void RedirectPageDetailHandler()
        {
            try
            {
                Dictionary<string, string> pParams = new Dictionary<string, string>
                {
                    { "pActionType", nameof(EnumType.Add) },
                    { "pDocEntry", "-1" },
                };
                string key = _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams)); // mã hóa key
                _navigationManager.NavigateTo($"/ho-so-nhan-vien?key={key}");
            }
            catch { }
        }

        /// <summary>
        /// xóa danh mục loại hợp đồng
        /// </summary>
        /// <returns></returns>
        public async Task DeleteDataHandler()
        {
            try
            {
                await checkPermission(MenuId);
                if (!IsAllowDelete)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
                IReadOnlyList<object>? lstEmployeeSelected = ActiveTabIndex == 0 ? SelectedEmployees : SelectedEmployeeOffs;
                if (lstEmployeeSelected.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                await ShowLoading();
                string tableName = _encryptHelper.Encrypt(nameof(EnumObjType.Employees));
                string pKey = _encryptHelper.Encrypt(nameof(LeaveConfigModel.id));
                string fKey = _encryptHelper.Encrypt(nameof(ContractModel.employeeId));
                string ids = string.Join(",", lstEmployeeSelected!.Cast<EmployeeModel>().Select(m => m.id));
                string reasonDelete = "";
                string strResult = await _masterDataService.DeleteDynnamicAsync(UserId, Token, BranchId, tableName, pKey, fKey, ids, reasonDelete);
                if (strResult == "-1") return;
                if (strResult == StatusCodes.Status200OK.ToString())
                {
                    await getEmployee();
                    SelectedEmployees = null;
                    SelectedEmployeeOffs = null;
                    return;
                }
                await Task.Delay(75);
                await ShowLoading(false);
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_NOTIFICATION, $"{strResult} ", isShowFooter: false);
                return;
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "DeleteDataHandler");
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
                    if (GridEmployee == null || ListEmployee.IsNullOrEmpty())
                    {
                        ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                        return;
                    }
                    await ShowLoading();
                    await GridEmployee!.ExportToXlsxAsync("Danh-sach-nhan-vien", new GridXlExportOptions()
                    {
                        ExportTotalSummaries = false,
                        ExportGroupSummaries = false
                    });
                    return;
                }
                if (GridEmployeeOff == null || ListEmployeeOff.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                await ShowLoading();
                await GridEmployeeOff!.ExportToXlsxAsync("Danh-sach-nhan-vien-nghi-viec", new GridXlExportOptions()
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
        
        /// <summary>
        /// Chọn cột hiển thị
        /// </summary>
        protected void ColumnChooseHandler()
        {
            try
            {
                if (ActiveTabIndex == 0)
                {
                    if (GridEmployee == null) return;
                    GridEmployee.ShowColumnChooser();
                    return;
                }
                if (GridEmployeeOff == null) return;
                GridEmployeeOff.ShowColumnChooser();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ExportExcelHandler");
                ShowError(ex.Message);
            }
        }
        #endregion

    }
}
