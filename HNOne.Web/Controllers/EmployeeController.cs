using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class EmployeeController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IPersonnelService _personnelService { get; init; }
        [Inject] IEncryptHelper _encryptHelper { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public EmployeeModel EmployeeUpdate { get; set; } = new EmployeeModel();
        public List<ComboboxModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public List<ComboboxModel>? ListCboManager { get; set; } // cbo ds người quản lý
        public List<ComboboxModel>? ListCboPosition { get; set; } // cbo ds người quản lý
        public List<ComboboxModel>? ListCboTitle { get; set; } // cbo ds người quản lý
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender)
            {
                try
                {
                    await ShowLoading();
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

        private void initDataAsync(bool isRefresh = false)
        {
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
                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask3
                );

                ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboPosition = (await getTask2)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboTitle = (await getTask3)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
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
            if (EmployeeUpdate.dateOfJoining == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ngày vào công ty");
                fieldName = nameof(EmployeeUpdate.dateOfJoining);
                return;
            }
            if (EmployeeUpdate.startDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ngày thử việc");
                fieldName = nameof(EmployeeUpdate.startDate);
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
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, MessageConstants.MESSAGE_CONFIRM_ADD);
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
