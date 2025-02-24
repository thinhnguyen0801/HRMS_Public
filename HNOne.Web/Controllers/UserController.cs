using DevExpress.Blazor;
using DevExpress.Pdf.Native;
using DevExpress.Pdf.Native.BouncyCastle.Asn1.Ocsp;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using HNOne.Web.Models;
using HNOne.Web.Services;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class UserController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IUserService _userDataService { get; init; }
        [Inject] IPersonnelService _personnelService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        [Inject] IEncryptHelper _encryptHelper { get; init; }
        public W1Confirm confirm { get; set; }

        const string STRING_KEY_EVENT_POST = "USER_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "USER_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "USER_CONTROLLER_DELETE";
        #region Properties
        public List<UserModel>? ListUser { get; set; }
        public IGrid? GridUser { get; set; }
        public IReadOnlyList<object>? SelectedUsers { get; set; } = null;
        public UserModel UserUpdate { get; set; } = new UserModel();
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;
        
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public IEnumerable<ComboboxModel>? ListCboBranchSelected { get; set; } // ds được chọn chi nhánh sử dụng
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public IEnumerable<ComboboxModel>? ListDepartmentSelected { get; set; }
        public List<ComboboxModel>? ListCboPerGroup { get; set; } // cbo ds quyền nhóm
        public List<EmployeeModel>? ListEmployee { get; set; } // ds nhân viên
        public string? StatusFilter { get; set; } // tình trạng
        public bool IsShowPassword { get; set; } = false;
        public bool IsShowRePassword { get; set; } = false;


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
                    string errMessage = await CheckMenuPermissionAsync("tai-khoan");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Hệ thống"),
                        new BreadcrumbModel("Tài khoản", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await buildComboAsync();
                    await getUsers();

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
        private async Task getUsers()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.branchId = BranchId;
            request.opt = "";
            request.branchIds = BranchIds;
            ListUser = new List<UserModel>();
            ListUser = await _userDataService.GetUserAsync(request);
        }

        private async Task buildComboAsync()
        {
            try
            {
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.branchId = BranchId;
                request.opt = "ACTIVE";
                request.branchIds = BranchIds;
                var getTask1 = _masterDataService.GetBranchAsync(UserId, Token, BranchId, $"{BranchIds}");
                var getTask2 = _masterDataService.GetDepartmentAsync(UserId, Token, BranchId, opt: CommonConstants.ENUM_ACTIVE);
                var getTask3 = _userDataService.GetPermissionGroup(request);
                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask3
                );
                ListCboBranch = (await getTask1)?.Select(m => new ComboboxModel() { id = m.branchId, code = m.branchCode, name = m.branchName })?.ToList();
                ListCboDepartment = (await getTask2)?.Select(m => new ComboboxModel() { id = m.id, code = m.code, name = m.name })?.ToList();
                ListCboPerGroup = (await getTask3)?.Select(m => new ComboboxModel() { id = m.id, code = m.code, name = m.name })?.ToList();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "buildComboAsync");
            }
        }

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(UserUpdate.userName))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Tên tài khoản");
                fieldName = "txtUserName";
                return;
            }
            if (string.IsNullOrEmpty(UserUpdate.password))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Mật khẩu");
                fieldName = "txtPassword";
                return;
            }
            if (string.IsNullOrEmpty(UserUpdate.rePassword))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Mật khẩu");
                fieldName = "txtRePassword";
                return;
            }
            if (UserUpdate.password.Trim() != UserUpdate.rePassword.Trim())
            {
                errorMessage = string.Format("Nhập lại mật khẩu không khớp!");
                fieldName = "txtRePassword";
                return;
            }
            if (UserUpdate.branchId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chi nhánh");
                fieldName = nameof(UserUpdate.branchId);
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
                await getUsers();
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

        protected async Task OnOpenDialogHandler(EnumType pAction = EnumType.Add, UserModel? pItemDetails = null)
        {
            try
            {
                await checkPermission(MenuId);
                IsShowPassword = false;
                IsShowRePassword = false;
                if (pAction == EnumType.Add)
                {
                    if (!IsAllowPost)
                    {
                        ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                        return;
                    }
                    IsCreate = true;
                    UserUpdate = new UserModel();
                }
                else
                {
                    if (!IsAllowPut)
                    {
                        ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                        return;
                    }
                    UserUpdate.userId = pItemDetails!.userId;
                    UserUpdate.userName = pItemDetails!.userName;
                    UserUpdate.password = _encryptHelper.Decrypt(pItemDetails!.password);
                    UserUpdate.rePassword = _encryptHelper.Decrypt(pItemDetails!.password);
                    UserUpdate.branchId = pItemDetails!.branchId;
                    UserUpdate.employeeId = pItemDetails!.employeeId;
                    UserUpdate.employeeCode = pItemDetails!.employeeCode;
                    UserUpdate.employeeName = pItemDetails!.employeeName;
                    UserUpdate.branchIds = pItemDetails!.branchIds;
                    string[] arrBranchSelected = $"{pItemDetails!.branchIds}".Split(",");
                    ListCboBranchSelected = ListCboBranch?.Where(m => arrBranchSelected.Contains($"{m.id}"));
                    UserUpdate.departmentIds = pItemDetails!.departmentIds;
                    string[] arrDepartSelected = $"{pItemDetails!.departmentIds}".Split(",");
                    ListDepartmentSelected = ListCboDepartment?.Where(m => arrDepartSelected.Contains($"{m.id}"));
                    UserUpdate.isActive = pItemDetails!.isActive;
                    UserUpdate.isAdmin = pItemDetails!.isAdmin;
                    UserUpdate.perGroupId = pItemDetails!.perGroupId;
                    IsCreate = false;
                }
                IsShowDialog = true;
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
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                    return;
                }
                errorMessage = IsCreate ? MessageConstants.MESSAGE_CONFIRM_ADD : MessageConstants.MESSAGE_CONFIRM_UPDATE;
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = IsCreate ? ProcessConstants.POST_USER : ProcessConstants.PUT_USER;
                UserUpdate.userSign = UserId;
                UserUpdate.userSign2 = UserId;
                if(!ListCboBranchSelected.IsNullOrEmpty()) UserUpdate.branchIds = string.Join(",", ListCboBranchSelected!.Select(m => m.id));
                if(!ListDepartmentSelected.IsNullOrEmpty()) UserUpdate.departmentIds = string.Join(",", ListDepartmentSelected!.Select(m => m.id));
                string content = JsonConvert.SerializeObject(UserUpdate);
                isConfirm = await _userDataService.UpdateUserAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getUsers();
                    IsShowDialog = false;
                    SelectedUsers = null;
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

        protected async Task DeleteDataHandler()
        {
            try
            {
                await checkPermission(MenuId);
                if (!IsAllowDelete)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
                if (SelectedUsers.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                await ShowLoading();
                string tableName = _encryptHelper.Encrypt(nameof(EnumObjType.Users));
                string pKey = _encryptHelper.Encrypt(nameof(UserModel.userId));
                string fKey = _encryptHelper.Encrypt(nameof(ContractModel.userSign));
                string ids = string.Join(",", SelectedUsers!.Cast<UserModel>().Select(m => m.userId));
                string reasonDelete = "";
                string strResult = await _masterDataService.DeleteDynnamicAsync(UserId, Token, BranchId, tableName, pKey, fKey, ids, reasonDelete);
                if (strResult == "-1") return;
                if (strResult == StatusCodes.Status200OK.ToString())
                {
                    await getUsers();
                    SelectedUsers = null;
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
        
        protected void ShowPasswordHandler() => IsShowPassword = !IsShowPassword;
        protected void ShowRePasswordHandler() => IsShowRePassword = !IsShowRePassword;

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
                UserUpdate.employeeId = employee.id;
                UserUpdate.employeeCode = employee.code;
                UserUpdate.employeeName = employee.name;
                IsShowDialogEmpSearch = false;
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

        protected async Task OpenPopupHandler(string type = nameof(EmployeeSelected), string popupType = nameof(UserUpdate.employeeCode))
        {
            try
            {
                switch (type)
                {
                    case nameof(EmployeeSelected):
                        ListCboDepartment ??= new();
                        DepartmentIds = string.Join(",", ListCboDepartment.Select(m => m.id));
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
        #endregion
    }
}
