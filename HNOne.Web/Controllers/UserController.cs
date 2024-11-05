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

        #region Properties
        public List<UserModel>? ListUser { get; set; }
        public IGrid? GridUser { get; set; }
        public IReadOnlyList<object>? SelectedUsers { get; set; } = null;
        public UserModel UserUpdate { get; set; } = new UserModel();
        public EditContext? _EditContext { get; set; }
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;
        public W1Confirm confirm { get; set; }
        public List<ComboboxModel>? ListCboBranchId { get; set; } // cbo ds chi nhánh
        public List<ComboboxModel>? ListCboDepartmentId { get; set; } // cbo ds chi nhánh
        public List<ComboboxModel>? ListPerGroupId { get; set; } // cbo ds quyền nhóm
        public List<EmployeeModel>? ListEmployee { get; set; } // ds nhân viên
        public List<ComboboxModel>? ListCboType { get; set; } // cbo ds loại lý do
        public string? StatusFilter { get; set; } // tình trạng
        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // danh sách tình trạng nhân viên
        public bool IsShowPassword { get; set; } = false;
        public bool IsShowRepassword { get; set; } = false;


        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Hệ thống"),
                        new BreadcrumbModel("Tài khoản", isActive: true)
                    };
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
            ListUser = new List<UserModel>();
            ListUser = await _userDataService.GetUserAsync(request);
        }
        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(UserUpdate.userName))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "tên tài khoản");
                fieldName = nameof(UserUpdate.userName);
                return;
            }
            if (string.IsNullOrEmpty(UserUpdate.password))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "mật khẩu");
                fieldName = nameof(UserUpdate.password);
                return;
            }
            if (UserUpdate.branchId < 1)
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chi nhánh");
                fieldName = nameof(UserUpdate.branchId);
                return;
            }
            if (UserUpdate.employeeId < 1)
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Nhân viên liên kết");
                fieldName = nameof(UserUpdate.employeeId);
                return;
            }
        }
        private async Task buildComboAsync()
        {
            try
            {
                ListCboStatus = await _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiNhanVien)); // ds trạng thái nhân viên
                if (!ListCboStatus.IsNullOrEmpty()) StatusFilter = ListCboStatus![0].code;
                var getTask1 = _masterDataService.GetBranchAsync(UserId, Token);
                var getTask2 = _masterDataService.GetDepartmentAsync(UserId, Token);
                await Task.WhenAll(
                        getTask1,
                        getTask2
                        );
                ListCboBranchId = (await getTask1)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
                ListCboDepartmentId = (await getTask2)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();

                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.branchId = BranchId;
                request.opt = "";
                ListEmployee = new List<EmployeeModel>();
                var lstEmp = await _personnelService.GetEmployeeAsync(request);
                ListEmployee = lstEmp?.Update(m =>
                {
                    Dictionary<string, string> pParams = new Dictionary<string, string>
                {
                    { "pActionType", nameof(EnumType.Update) },
                    { "pDocEntry", $"{m.id}" },
                };
                    m.link = _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
                })?.ToList();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "buildComboAsync");
            }
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

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, UserModel? pItemDetails = null)
        {
            try
            {
                IsShowPassword = false;
                IsShowRepassword = false;
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    UserUpdate = new UserModel();
                }
                else
                {
                    //string userName = _encryptHelper.Encrypt(LoginRequest.userName);

                    UserUpdate.userId = pItemDetails!.userId;
                    UserUpdate.userName = pItemDetails!.userName;
                    UserUpdate.password = _encryptHelper.Decrypt(pItemDetails!.password);
                    UserUpdate.branchId = pItemDetails!.branchId;
                    UserUpdate.employeeId = pItemDetails!.employeeId;
                    UserUpdate.branchIds = pItemDetails!.branchIds;
                    UserUpdate.departmentIds = pItemDetails!.departmentIds;
                    UserUpdate.isActive = pItemDetails!.isActive;
                    UserUpdate.isAdmin = pItemDetails!.isAdmin;
                    UserUpdate.perGroupId = pItemDetails!.perGroupId;
                    IsCreate = false;
                }
                IsShowDialog = true;
                _EditContext = new EditContext(UserUpdate);
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
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, MessageConstants.MESSAGE_CONFIRM_ADD);
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = IsCreate ? ProcessConstants.POST_USER : ProcessConstants.PUT_USER;
                UserUpdate.userSign = UserId;
                UserUpdate.userSign2 = UserId;
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

        public async Task DeleteDataHandler()
        {
            try
            {
                if (SelectedUsers.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                //isConfirm = await _masterDataService.UpdateUserAsync(processKey, UserId, Token, content);
                //if (isConfirm)
                //{
                //    await getUsers();
                //    IsShowDialog = false;
                //    SelectedUsers = null;
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
        #endregion
    }
}
