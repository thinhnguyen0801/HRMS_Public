using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class EmployeeInfoController : DocumentControllerBase
    {
        [Inject] IWorkforceService _workforceService { get; init; }
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IPersonnelService _personnelService { get; init; }
        [Inject] IEncryptHelper _encryptHelper { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        [Inject] IWebHostEnvironment _webHostEnvironment { get; init; }
        [Inject] IConfiguration _configuration { get; init; }
        public W1Confirm confirm { get; set; }
        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public int ActiveTabIndex { get; set; } = 0;
        public bool firstRender = true;
        public EmployeeModel EmployeeUpdate { get; set; } = new EmployeeModel();
        public List<FamilyRelationshipModel>? ListFamilyRelationship { get; set; } // danh sách quan hệ gia đình
        public IGrid? GridFamilyRelationship { get; set; }
        public FamilyRelationshipModel FamilyRelationshipUpdate { get; set; } = new FamilyRelationshipModel();

        //
        public List<EnumCatagoryModel>? ListCboRelationship { get; set; } // cbo ds quan hệ
        public List<ComboboxModel>? ListCboProvince { get; set; } // cbo ds tỉnh thành trong thông tin liên hệ
        public List<ComboboxModel>? ListCboCountry { get; set; } // cbo quốc gia
        public List<ComboboxModel>? ListCboProvince1 { get; set; } // cbo ds tỉnh thành 
        public List<ComboboxModel>? ListCboDistrict1 { get; set; } // cbo ds quận huyện 
        public List<ComboboxModel>? ListCboWard1 { get; set; } // cbo ds phường xã 

        public List<ComboboxModel>? ListCboProvince2 { get; set; } // cbo ds tỉnh thành 
        public List<ComboboxModel>? ListCboDistrict2 { get; set; } // cbo ds quận huyện 
        public List<ComboboxModel>? ListCboWard2 { get; set; } // cbo ds phường xã 

        public bool IsCreatePopup { get; set; }
        public bool IsShowPopupFamily { get; set; } // popup thêm mới Thông tin quan hệ gia đình
        #endregion

        #region Private Functions
        /// <summary>
        /// lấy danh sách tỉnh thành
        /// </summary>
        /// <param name="countryCode"></param>
        /// <returns></returns>
        private async Task<List<ComboboxModel>?> getProvince(string countryCode)
            => await _masterDataService.GetLocationAsync(UserId, Token, nameof(EnumCatagory.Province), countryCode);

        private async Task<List<ComboboxModel>?> getDistrict(string provinceCode)
            => await _masterDataService.GetLocationAsync(UserId, Token, nameof(EnumCatagory.District), opt1: provinceCode);

        private async Task<List<ComboboxModel>?> getWard(string provinceCode, string districtCode)
            => await _masterDataService.GetLocationAsync(UserId, Token, nameof(EnumCatagory.Ward), opt1: provinceCode, opt2: districtCode);

        /// <summary>
        /// lấy danh sách quan hệ gia đình
        /// </summary>
        /// <returns></returns>
        private async Task getFamilyRelationship()
        {
            RequestModel request = new RequestModel();
            request.employeeId = EmployeeUpdate.id;
            request.userId = UserId;
            request.token = Token;
            request.branchId = BranchId;
            ListFamilyRelationship = new List<FamilyRelationshipModel>();
            ListFamilyRelationship = await _personnelService.GetFamilyRelationshipAsync(request);
        }

        private void validateForSaveFamilyRelationship(ref string errorMessage, ref string fieldName)
        {
            if (EmployeeUpdate.id < 1)
            {
                errorMessage = "Vui lòng lưu thông tin trước khi thêm Quan hệ gia đình";
                fieldName = "";
                return;
            }
            if (string.IsNullOrEmpty(FamilyRelationshipUpdate.relationshipId))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Mối quan hệ");
                fieldName = "txtFamilyRelationshipId";
                return;
            }
            if (string.IsNullOrEmpty(FamilyRelationshipUpdate.name))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Họ tên");
                fieldName = "txtFamilyRelationshipName";
                return;
            }
        }
        #endregion Private Functions

        #region Protected Functions

        protected async Task OpenPopupHandler(string type = nameof(IsShowPopupFamily), string popupType = nameof(EmployeeUpdate.managerCode)
            , EnumType pAction = EnumType.Add, object? pItemDetails = null)
        {
            try
            {
                switch (type)
                {
                    case nameof(IsShowPopupFamily):
                        if (pAction == EnumType.Add)
                        {
                            IsCreatePopup = true;
                            FamilyRelationshipUpdate = new FamilyRelationshipModel();
                        }
                        else
                        {
                            FamilyRelationshipModel family = JsonConvert.DeserializeObject<FamilyRelationshipModel>
                                    (JsonConvert.SerializeObject(pItemDetails))!;
                            FamilyRelationshipUpdate.id = family.id;
                            FamilyRelationshipUpdate.employeeId = family.employeeId;
                            FamilyRelationshipUpdate.name = family.name;
                            FamilyRelationshipUpdate.relationshipId = family.relationshipId;
                            FamilyRelationshipUpdate.relationshipName = family.relationshipName;
                            FamilyRelationshipUpdate.dateOfBirth = family.dateOfBirth;
                            FamilyRelationshipUpdate.placeOfBirth = family.placeOfBirth;
                            FamilyRelationshipUpdate.occupation = family.occupation;
                            FamilyRelationshipUpdate.placeOfOrigin = family.placeOfOrigin;
                            FamilyRelationshipUpdate.temporaryAddress = family.temporaryAddress;
                            FamilyRelationshipUpdate.contactAddress = family.contactAddress;
                            FamilyRelationshipUpdate.phoneNumber = family.phoneNumber;
                            FamilyRelationshipUpdate.cIC = family.cIC;
                            FamilyRelationshipUpdate.issuanceDateCIC = family.issuanceDateCIC;
                            FamilyRelationshipUpdate.remark = family.remark;
                        }
                        IsShowPopupFamily = true;
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
        /// Scroll đến section của element
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        protected async Task ScrollToSection(string sectionId)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("scrollToSection", sectionId);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "ScrollToSection");
            }
        }

        /// <summary>
        /// Lưu thông tin nhân viên
        /// </summary>
        /// <returns></returns>
        protected async Task SaveDataHandler()
        {

        }

        /// <summary>
        /// Thay đổi giá trị combobox
        /// </summary>
        /// <param name="value"></param>
        /// <param name="controlID"></param>
        /// <returns></returns>
        protected async Task ComboboxValueChangedHandler(object? value
            , string controlID = nameof(EmployeeUpdate.countryCode1))
        {
            try
            {
                if (firstRender) return;
                switch (controlID)
                {
                    // chọn dữ liệu ở Hộ khẩu thường trú
                    case nameof(EmployeeUpdate.countryCode1):
                        await ShowLoading();
                        await Task.Delay(75);
                        ListCboProvince1 = await getProvince($"{value}");
                        ListCboDistrict1 = new List<ComboboxModel>();
                        ListCboWard1 = new List<ComboboxModel>();
                        EmployeeUpdate.countryCode1 = $"{value}";
                        EmployeeUpdate.provinceCode1 = string.Empty;
                        EmployeeUpdate.districtCode1 = string.Empty;
                        EmployeeUpdate.wardCode1 = string.Empty;
                        EmployeeUpdate.placeOfResidence = string.Empty;
                        break;
                    case nameof(EmployeeUpdate.provinceCode1):
                        await ShowLoading();
                        await Task.Delay(75);
                        ListCboDistrict1 = await getDistrict($"{value}");
                        ListCboWard1 = new List<ComboboxModel>();
                        EmployeeUpdate.provinceCode1 = $"{value}";
                        EmployeeUpdate.districtCode1 = string.Empty;
                        EmployeeUpdate.wardCode1 = string.Empty;
                        EmployeeUpdate.placeOfResidence = string.Empty;
                        break;
                    case nameof(EmployeeUpdate.districtCode1):
                        await ShowLoading();
                        await Task.Delay(75);
                        ListCboWard1 = await getWard($"{EmployeeUpdate.provinceCode1}", $"{value}");
                        EmployeeUpdate.districtCode1 = $"{value}";
                        EmployeeUpdate.wardCode1 = string.Empty;
                        EmployeeUpdate.placeOfResidence = string.Empty;
                        break;
                    // Chỗ ở hiện nay
                    case nameof(EmployeeUpdate.countryCode2):
                        await ShowLoading();
                        await Task.Delay(75);
                        ListCboProvince2 = await getProvince($"{value}");
                        ListCboDistrict2 = new List<ComboboxModel>();
                        ListCboWard2 = new List<ComboboxModel>();
                        EmployeeUpdate.countryCode2 = $"{value}";
                        EmployeeUpdate.provinceCode2 = string.Empty;
                        EmployeeUpdate.districtCode2 = string.Empty;
                        EmployeeUpdate.wardCode2 = string.Empty;
                        EmployeeUpdate.temporaryAddress = string.Empty;
                        break;
                    case nameof(EmployeeUpdate.provinceCode2):
                        await ShowLoading();
                        await Task.Delay(75);
                        ListCboDistrict2 = await getDistrict($"{value}");
                        ListCboWard2 = new List<ComboboxModel>();
                        EmployeeUpdate.provinceCode2 = $"{value}";
                        EmployeeUpdate.districtCode2 = string.Empty;
                        EmployeeUpdate.wardCode2 = string.Empty;
                        EmployeeUpdate.temporaryAddress = string.Empty;
                        break;
                    case nameof(EmployeeUpdate.districtCode2):
                        await ShowLoading();
                        await Task.Delay(75);
                        ListCboWard2 = await getWard($"{EmployeeUpdate.provinceCode2}", $"{value}");
                        EmployeeUpdate.districtCode2 = $"{value}";
                        EmployeeUpdate.wardCode2 = string.Empty;
                        EmployeeUpdate.temporaryAddress = string.Empty;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "ComboboxValueChangedHandler");
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// check giống hộ khẩu thường trú
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected void EqualsHouseholdCheckedChangedHandler(bool value)
        {
            try
            {
                EmployeeUpdate.isEqualsHousehold = value;
                ListCboProvince2 = ListCboProvince1;
                ListCboDistrict2 = ListCboDistrict1;
                ListCboWard2 = ListCboWard1;
                EmployeeUpdate.countryCode2 = EmployeeUpdate.countryCode1;
                EmployeeUpdate.provinceCode2 = EmployeeUpdate.provinceCode1;
                EmployeeUpdate.districtCode2 = EmployeeUpdate.districtCode1;
                EmployeeUpdate.wardCode2 = EmployeeUpdate.wardCode1;
                EmployeeUpdate.houseNumber2 = EmployeeUpdate.houseNumber1;
                EmployeeUpdate.temporaryAddress = EmployeeUpdate.placeOfResidence;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "EqualsHouseholdCheckedChangedHandler");
            }
        }

        protected async Task RefreshHandler(string process = ProcessConstants.GET_EDUCATION)
        {
            try
            {
                await ShowLoading();
                await Task.Yield();
                switch (process)
                {
                    case nameof(IsShowPopupFamily):
                        await getFamilyRelationship();
                        break;
                }
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

        protected async Task SaveFamilyRelationshipHandler()
        {
            try
            {
                string errorMessage = string.Empty;
                string fieldName = string.Empty;
                validateForSaveFamilyRelationship(ref errorMessage, ref fieldName);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                    return;
                }
                errorMessage = IsCreatePopup ? MessageConstants.MESSAGE_CONFIRM_ADD : MessageConstants.MESSAGE_CONFIRM_UPDATE;
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = IsCreatePopup ? ProcessConstants.POST_FAMILYRELATIONSHIP : ProcessConstants.PUT_FAMILYRELATIONSHIP;
                FamilyRelationshipUpdate.employeeId = EmployeeUpdate.id;
                FamilyRelationshipUpdate.userSign = UserId;
                FamilyRelationshipUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(FamilyRelationshipUpdate);
                isConfirm = await _personnelService.UpdateFamilyRelationshipAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getFamilyRelationship();
                    IsShowPopupFamily = false;
                }
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "SaveFamilyRelationshipHandler");
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
