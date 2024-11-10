using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class DepartmentController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        const string STRING_KEY_EVENT_POST = "DEPARTMENT_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "DEPARTMENT_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "DEPARTMENT_CONTROLLER_DELETE";
        #region Properties

        public List<DepartmentModel>? ListDepartment { get; set; }
        public IGrid? GridDepartment { get; set; }
        public IReadOnlyList<object>? SelectedDepartments { get; set; } = null;
        public DepartmentModel DepartmentUpdate { get; set; } = new DepartmentModel();
        public EditContext? _EditContext { get; set; }
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;
        public W1Confirm confirm { get; set; }
        public List<ComboboxModel>? ListCboHead { get; set; } // cbo ds trưởng phòng
        public List<ComboboxModel>? ListCboManager { get; set; } // cbo ds giám đốc
        public List<ComboboxModel>? ListCboAssistantManager { get; set; } // cbo ds phó phòng
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh

        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public bool IsShowDialogEmpSearch { get; set; }
        public string? DepartmentIds { get; set; }
        public string? StatusIds { get; set; } // Tình trạng nào
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn
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
                    string errMessage = await CheckMenuPermissionAsync("phong-ban");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Phòng ban", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await buildComboboxAsync();
                    await getDepartments();

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
        private async Task buildComboboxAsync()
        {
            try
            {
                var getTask1 = _masterDataService.GetBranchAsync(UserId, Token);
                await Task.WhenAll(
                    getTask1
                    );
                ListCboBranch = (await getTask1)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "BuildComboAsync");
            }
        }
        private async Task getDepartments()
        {
            ListDepartment = new List<DepartmentModel>();
            ListDepartment = await _masterDataService.GetDepartmentAsync(UserId, Token);
        }

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if(string.IsNullOrEmpty(DepartmentUpdate.code))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Mã phòng ban");
                fieldName = "txtCode";
                return;
            }
            if(string.IsNullOrEmpty(DepartmentUpdate.name))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Tên phòng ban");
                fieldName = "txtName";
                return;
            }
            if (DepartmentUpdate.branchId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chi nhánh");
                fieldName = "txtBranchId";
                return;
            }
        }

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
        #endregion

        #region
        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getDepartments();
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

        protected async Task OnOpenDialogHandler(EnumType pAction = EnumType.Add, DepartmentModel? pItemDetails = null)
        {
            try
            {
                await checkPermission(MenuId);
                if (pAction == EnumType.Add)
                {
                    if (!IsAllowPost)
                    {
                        ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                        return;
                    }
                    IsCreate = true;
                    DepartmentUpdate = new DepartmentModel();
                    if (!ListCboBranch.IsNullOrEmpty()) DepartmentUpdate.branchId = BranchId;
                }
                else
                {
                    if (!IsAllowPut)
                    {
                        ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                        return;
                    }
                    DepartmentUpdate.id = pItemDetails!.id;
                    DepartmentUpdate.code = pItemDetails!.code;
                    DepartmentUpdate.name = pItemDetails!.name;
                    DepartmentUpdate.managerId = pItemDetails!.managerId;
                    DepartmentUpdate.headId = pItemDetails!.headId;
                    DepartmentUpdate.assistantManagerIds = pItemDetails!.assistantManagerIds;
                    DepartmentUpdate.remark = pItemDetails!.remark;
                    DepartmentUpdate.isActive = pItemDetails!.isActive;
                    DepartmentUpdate.branchId = pItemDetails!.branchId;
                    IsCreate = false;
                }
                IsShowDialog = true;
                _EditContext = new EditContext(DepartmentUpdate);
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "OnOpenDialogHandler");
                ShowError(ex.Message);
            }
        }

        protected async Task SaveDataHandler()
        {
            try
            {
                string errorMessage = string.Empty;
                string fieldName = string.Empty;
                validateForSave(ref errorMessage, ref fieldName);
                if(!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                    return;
                }
                errorMessage = IsCreate ? MessageConstants.MESSAGE_CONFIRM_ADD : MessageConstants.MESSAGE_CONFIRM_UPDATE;
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = IsCreate ? ProcessConstants.POST_DEPARTMENT : ProcessConstants.PUT_DEPARTMENT;
                DepartmentUpdate.userSign = UserId;
                DepartmentUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(DepartmentUpdate);
                isConfirm = await _masterDataService.UpdateDepartmentAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getDepartments();
                    IsShowDialog = false;
                    SelectedDepartments = null;
                }
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
                if (SelectedDepartments.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                //isConfirm = await _masterDataService.UpdateDepartmentAsync(processKey, UserId, Token, content);
                //if (isConfirm)
                //{
                //    await getDepartments();
                //    IsShowDialog = false;
                //    SelectedDepartments = null;
                //}
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


        protected async Task OpenPopupHandler(string type = nameof(EmployeeSelected), string popupType = nameof(DepartmentUpdate.headCode))
        {
            try
            {
                pPopupType = popupType;
                switch (type)
                {
                    case nameof(EmployeeSelected):
                        DepartmentIds = "";
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
        /// callback nhân viên
        /// </summary>
        /// <param name="lstEmp"></param>
        protected void EventCallbackEmpChangedHandler(object? lstEmp) => EmployeeSelected = lstEmp;

        /// <summary>
        /// chọn nhân viên
        /// </summary>
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
                    case nameof(DepartmentUpdate.headCode):
                        DepartmentUpdate.headId = employee.id;
                        DepartmentUpdate.headCode = employee.code;
                        DepartmentUpdate.headName = employee.name;
                        IsShowDialogEmpSearch = false;
                        break;
                    case nameof(DepartmentUpdate.assistantManagerCode):
                        DepartmentUpdate.assistantManagerIds = employee.id.ToString();
                        DepartmentUpdate.assistantManagerCode = employee.code;
                        DepartmentUpdate.assistantManagerName = employee.name;
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
        #endregion
    }
}
