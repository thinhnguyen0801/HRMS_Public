using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public partial class EmployeeController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IPersonnelService _personnelService { get; init; }
        [Inject] IEncryptHelper _encryptHelper { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public int ActiveTabIndex { get; set; } = 0;
        public EmployeeModel EmployeeUpdate { get; set; } = new EmployeeModel();
        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public List<ComboboxModel>? ListCboPosition { get; set; } // cbo ds chức vụ
        public List<ComboboxModel>? ListCboTitle { get; set; } // cbo ds chức danh
        public List<EnumCatagoryModel>? ListCboMaritalStatus { get; set; } // cbo ds chức danh
        public List<EnumCatagoryModel>? ListCboProvince { get; set; } // cbo ds tỉnh thành


        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public bool IsShowDialogEmpSearch { get; set; }
        public string? DepartmentIds { get; set; }
        public string? StatusIds { get; set; } // Tình trạng nào
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if(firstRender)
            {
                try
                {
                    await ShowLoading();
                    await initDataAsync();
                    await buildComboAsync();
                    if (pDocEntry > 0) await showVoucher();
                }
                catch (Exception) { }
                finally
                {
                    await ShowLoading(false);
                    await InvokeAsync(StateHasChanged);
                }
            }    
        }

        #region Private Functions

        private async Task initDataAsync(bool isRefresh = false)
        {
            ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Nhân sự"),
                        new BreadcrumbModel("Danh sách nhân viên", enpoint: "/danh-sach-nhan-vien"),
                        new BreadcrumbModel("Hồ sơ nhân viên", isActive: true)
                    };
            await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);

            var uri = _navigationManager?.ToAbsoluteUri(_navigationManager.Uri);
            if(uri != null && QueryHelpers.ParseQuery(uri.Query).Count > 0)
            {
                string key = uri.Query.Substring(5); // bỏ ?key=
                Dictionary<string, string> pParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(_encryptHelper.Decrypt(key))!;
                if (pParams != null && pParams.Any())
                {
                    if (pParams.ContainsKey("pActionType")) pActionType = Convert.ToString(pParams["pActionType"]);
                    if (pParams.ContainsKey("pDocEntry")) pDocEntry = Convert.ToInt32(pParams["pDocEntry"]);
                }
            }    
        }

        /// <summary>
        /// lấy danh sách dữ liệu combobox
        /// </summary>
        /// <returns></returns>
        private async Task buildComboAsync()
        {
            try
            {
                var getTask1 = _masterDataService.GetDepartmentAsync(UserId, Token); // ds phòng ban
                var getTask2 = _masterDataService.GetPositionAsync(UserId, Token); // ds chức vụ
                var getTask3 = _masterDataService.GetTitleAsync(UserId, Token); // ds chức danh
                var getTask4 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiNhanVien)); // ds trạng thái
                var getTask5 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.TinhTrangHonNhan)); // ds trạng thái

                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask3,
                    getTask4,
                    getTask5
                );

                ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboPosition = (await getTask2)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboTitle = (await getTask3)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboMaritalStatus = await getTask5;
                ListCboStatus = (await getTask4)?.Where(m => m.rowOrder != 0).ToList();
                if (!ListCboStatus.IsNullOrEmpty()) EmployeeUpdate.statusId = ListCboStatus![0].code;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "buildComboAsync");
            }
        }

        private async Task showVoucher()
        {
            try
            {
                RequestModel request = new RequestModel();
                request.employeeId = pDocEntry;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                var lstData = await _personnelService.GetEmployeeAsync(request);
                if(!lstData.IsNullOrEmpty()) EmployeeUpdate = lstData![0];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(EmployeeUpdate.name))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Họ tên");
                fieldName = nameof(EmployeeUpdate.name);
                return;
            }
            if (EmployeeUpdate.departmentId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Phòng ban");
                fieldName = nameof(EmployeeUpdate.departmentId);
                return;
            }
            if (EmployeeUpdate.positionId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chức vụ");
                fieldName = nameof(EmployeeUpdate.positionId);
                return;
            }
            if (EmployeeUpdate.dateOfBirth == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ngày sinh");
                fieldName = nameof(EmployeeUpdate.dateOfBirth);
                return;
            }
            if (string.IsNullOrEmpty(EmployeeUpdate.gender))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Giới tính");
                fieldName = nameof(EmployeeUpdate.gender);
                return;
            }
            if (string.IsNullOrEmpty(EmployeeUpdate.cIC))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "CCCD/CMND");
                fieldName = nameof(EmployeeUpdate.cIC);
                return;
            }
            //if (EmployeeUpdate.dateOfJoining == null)
            //{
            //    errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ngày vào công ty");
            //    fieldName = nameof(EmployeeUpdate.dateOfJoining);
            //    return;
            //}
            if (EmployeeUpdate.probationStartDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ngày thử việc");
                fieldName = nameof(EmployeeUpdate.probationStartDate);
                return;
            }
            if (EmployeeUpdate.probationEndDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ngày kết thúc thử việc");
                fieldName = nameof(EmployeeUpdate.startDate);
                return;
            }
        }
        #endregion

        #region Projected
        /// <summary>
        /// hiển thị các popup
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected async Task OpenPopupHandler(string type = nameof(EmployeeSelected), string popupType = nameof(EmployeeUpdate.managerCode))
        {
            try
            {
                pPopupType = popupType;
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
                    case nameof(EmployeeUpdate.managerCode):
                        EmployeeUpdate.managerId = employee.id;
                        EmployeeUpdate.managerCode = employee.code;
                        EmployeeUpdate.managerName = employee.name;
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

        protected async Task SaveDataHandler()
        {
            try
            {
                string errorMessage = string.Empty;
                string fieldName = string.Empty; // trả ra trường nào cần validate
                validateForSave(ref errorMessage, ref fieldName);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                    return;
                }
                bool isConfirm = true;
                errorMessage = pActionType == nameof(EnumType.Add) ? MessageConstants.MESSAGE_CONFIRM_ADD : MessageConstants.MESSAGE_CONFIRM_UPDATE;
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = pActionType == nameof(EnumType.Add) ? ProcessConstants.POST_EMPLOYEE : ProcessConstants.PUT_EMPLOYEE;
                EmployeeUpdate.branchId = BranchId;
                EmployeeUpdate.userSign = UserId;
                EmployeeUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(EmployeeUpdate);
                isConfirm = await _personnelService.UpdateEmployeeAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await showVoucher();
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
        #endregion
    }
}
