using DevExpress.Blazor;
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
    public class EmployeeInfoController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IPersonnelService _personnelService { get; init; }
        [Inject] IEncryptHelper _encryptHelper { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        [Inject] IWebHostEnvironment _webHostEnvironment { get; init; }
        [Inject] IConfiguration _configuration { get; init; }
        [Inject] IUserService _userDataService { get; init; }
        public W1Confirm confirm { get; set; }
        #region Properties
        public int ActiveTabIndex { get; set; } = 0;
        public int ActiveTabMainIndex { get; set; } = 0;
        public bool firstRender = true;
        public EmployeeModel EmployeeUpdate { get; set; } = new EmployeeModel();
        public EmployeeModel EmployeeRefUpdate { get; set; } = new EmployeeModel(); // employee này gán dữ liệu để đi cập nhật
        public List<InsuranceModel>? ListInsurance { get; set; } // danh sách thông tin bảo hiểm
        public List<ContractModel>? ListContract { get; set; } // danh sách hợp đồng
        public List<FamilyRelationshipModel>? ListFamilyRelationship { get; set; } // danh sách quan hệ gia đình
        public IGrid? GridFamilyRelationship { get; set; }
        public FamilyRelationshipModel FamilyRelationshipUpdate { get; set; } = new FamilyRelationshipModel();
        public UserModel UserUpdate { get; set; } = new UserModel(); // cập nhật thông tin đăng nhập

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
        public bool IsShowPopupChangePass { get; set; } // popup đổi mật khẩu
        public bool IsShowPopupUpdateEmployee { get; set; } // popup thay đổi thông tin nhân viên
        public string? PopupTypeUpdateEmployee { get; set; }
        public string? HeaderTextUpdateEmployee { get; set; }

        // nút quyền
        public bool IsAllowPut { get; set; }
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    this.firstRender = firstRender;
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Thông tin nhân viên")
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);

                    //
                    await buildComboAsync();
                    await showVoucher();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OnAfterRenderAsync");
                    ShowError(ex.Message);
                }
                finally
                {
                    this.firstRender = false;
                    await ShowLoading(false);
                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        #region Private Functions

        private async Task buildComboAsync()
        {
            try
            {
                var getTask5 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.CapNhatThongTinNhanVien)); // lấy lên cấu hình thông tin nhân viên
                var getTask6 = _masterDataService.GetLocationAsync(UserId, Token, nameof(EnumCatagory.County)); // ds quốc gia
                var getTask7 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.QuanHeGiaDinh)); // ds quan hệ gia đình
                var getTask8 = _masterDataService.GetLocationAsync(UserId, Token, nameof(EnumCatagory.Province), "VN"); // ds tỉnh thành
                await Task.WhenAll(
                    getTask5,
                    getTask6,
                    getTask7,
                    getTask8
                );
                ListCboCountry = await getTask6;
                ListCboRelationship = await getTask7;
                ListCboProvince = await getTask8;
                var allowUpdate = (await getTask5)?.FirstOrDefault();
                IsAllowPut = allowUpdate?.value == GlobalContants.ENUM_YES;
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
                if (EmployeeId < 1) return;
                RequestModel request = new RequestModel();
                request.employeeId = EmployeeId;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                var lstData = await _personnelService.GetEmployeeAsync(request);
                if (!lstData.IsNullOrEmpty())
                {
                    EmployeeUpdate = lstData![0];
                    if (!string.IsNullOrEmpty(EmployeeUpdate.imageUrl))
                    {
                        string apiUrl = _configuration.GetSection("appSettings:ImageUrl").Value + "";
                        EmployeeUpdate.imageViewUrl = $"{apiUrl}{nameof(EmployeeController)}/{EmployeeUpdate.imageUrl}";
                    }
                    List<Task> lstTask = new List<Task>();
                    // nếu có chọn quốc tịch
                    if (!string.IsNullOrEmpty(EmployeeUpdate.countryCode1))
                        lstTask.Add(getProvince(EmployeeUpdate.countryCode1).ContinueWith(t => ListCboProvince1 = t.Result));

                    // nếu có chọn tỉnh thành
                    if (!string.IsNullOrEmpty(EmployeeUpdate.provinceCode1))
                        lstTask.Add(getDistrict(EmployeeUpdate.provinceCode1).ContinueWith(t => ListCboDistrict1 = t.Result));

                    // nếu có chọn quận huyện
                    if (!string.IsNullOrEmpty(EmployeeUpdate.districtCode1))
                        lstTask.Add(getWard($"{EmployeeUpdate.provinceCode1}", EmployeeUpdate.districtCode1).ContinueWith(t => ListCboWard1 = t.Result));

                    // nếu có chọn quốc tịch
                    if (!string.IsNullOrEmpty(EmployeeUpdate.countryCode2))
                        lstTask.Add(getProvince(EmployeeUpdate.countryCode2).ContinueWith(t => ListCboProvince2 = t.Result));

                    // nếu có chọn tỉnh thành
                    if (!string.IsNullOrEmpty(EmployeeUpdate.provinceCode2))
                        lstTask.Add(getDistrict(EmployeeUpdate.provinceCode2).ContinueWith(t => ListCboDistrict2 = t.Result));

                    // nếu có chọn quận huyện
                    if (!string.IsNullOrEmpty(EmployeeUpdate.districtCode2))
                        lstTask.Add(getWard($"{EmployeeUpdate.provinceCode2}", EmployeeUpdate.districtCode2).ContinueWith(t => ListCboWard2 = t.Result));

                    lstTask.Add(geInsurance()); // danh sách hợp đồng
                    lstTask.Add(getFamilyRelationship()); // danh sách quan hệ gia đình
                    //lstTask.Add(getEducation()); // danh sách trình độ đại học
                    lstTask.Add(getContractList()); // danh sách hợp đồng

                    await Task.WhenAll(lstTask);
                    IsShowPopupUpdateEmployee = false;
                }
            }
            catch (Exception) { throw; }
        }

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

        /// <summary>
        /// lấy danh sách hợp đồng theo nhân viên
        /// </summary>
        /// <returns></returns>
        private async Task getContractList()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.branchId = BranchId;
            request.token = Token;
            request.type = "BY_EMPLOYEE";
            request.opt = ActiveTabIndex == 0 ? "ACTIVE" : "";
            request.employeeId = EmployeeUpdate.id;
            var lstContract = await _personnelService.GetContractAsync(request, isShowToast: false);
            lstContract = lstContract?.Update(m =>
            {
                Dictionary<string, string> pParams = new Dictionary<string, string>
                {
                    { "pActionType", nameof(EnumType.Update) },
                    { "pDocEntry", $"{m.id}" },
                };
                m.link = _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
            })?.ToList();
            ListContract = lstContract;
        }

        /// <summary>
        /// lấy danh sách bảo hiểm
        /// </summary>
        /// <returns></returns>
        private async Task geInsurance()
        {
            RequestModel request = new RequestModel();
            request.employeeId = EmployeeUpdate.id;
            request.userId = UserId;
            request.token = Token;
            request.branchId = BranchId;
            ListInsurance = new List<InsuranceModel>();
            ListInsurance = await _personnelService.GetInsuranceAsync(request);
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

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(EmployeeRefUpdate.cIC))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "CCCD/CMND");
                fieldName = nameof(EmployeeRefUpdate.cIC);
                return;
            }
        }

        /// <summary>
        /// lấy thông tin nhân viên theo id
        /// </summary>
        /// <returns></returns>
        private async Task getUserById()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.branchId = BranchId;
            request.opt = "";
            request.documentId = UserId;
            var listUser = await _userDataService.GetUserAsync(request);
            if (!listUser.IsNullOrEmpty())
            {
                UserUpdate = listUser![0];
                IsShowPopupChangePass = true;
            }
        }
        #endregion Private Functions

        #region Protected Functions

        protected async Task OpenPopupHandler(string type = nameof(IsShowPopupFamily), string popupType = nameof(EmployeeUpdate.managerCode)
            , EnumType pAction = EnumType.Add, object? pItemDetails = null, string headerText = "")
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
                            IsCreatePopup = false;
                        }
                        IsShowPopupFamily = true;
                        break;
                    case nameof(IsShowPopupChangePass):
                        await ShowLoading();
                        await getUserById();
                        break;
                    case nameof(IsShowPopupUpdateEmployee):
                        PopupTypeUpdateEmployee = popupType;
                        HeaderTextUpdateEmployee = headerText;
                        EmployeeRefUpdate = new EmployeeModel();
                        var employee = JsonConvert.DeserializeObject<EmployeeModel>(JsonConvert.SerializeObject(EmployeeUpdate));
                        if (employee == null) return;
                        EmployeeRefUpdate = employee;
                        IsShowPopupUpdateEmployee = true;
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
                errorMessage = string.Format(MessageConstants.MESSAGE_CONFIRM_UPDATE_FORMAT, "thông tin nhân viên");
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = ProcessConstants.PUT_EMPLOYEE_INFO;
                if (!string.IsNullOrEmpty(EmployeeRefUpdate.provinceCode))
                    EmployeeRefUpdate.provinceName = ListCboProvince?.FirstOrDefault(m => m.code == EmployeeRefUpdate.provinceCode)?.name;
                // Lấy tên thông tin hộ khẩu thường trú
                if (!string.IsNullOrEmpty(EmployeeRefUpdate.countryCode1))
                    EmployeeRefUpdate.countryName1 = ListCboCountry?.FirstOrDefault(m => m.code == EmployeeRefUpdate.countryCode1)?.name;
                if (!string.IsNullOrEmpty(EmployeeRefUpdate.provinceCode1))
                    EmployeeRefUpdate.provinceName1 = ListCboProvince1?.FirstOrDefault(m => m.code == EmployeeRefUpdate.provinceCode1)?.name;
                if (!string.IsNullOrEmpty(EmployeeRefUpdate.districtCode1))
                    EmployeeRefUpdate.districtName1 = ListCboDistrict1?.FirstOrDefault(m => m.code == EmployeeRefUpdate.districtCode1)?.name;
                if (!string.IsNullOrEmpty(EmployeeRefUpdate.wardCode1))
                    EmployeeRefUpdate.wardName1 = ListCboDistrict1?.FirstOrDefault(m => m.code == EmployeeRefUpdate.wardCode1)?.name;

                // Lấy tên thông tin Chỗ ở hiện nay
                if (!string.IsNullOrEmpty(EmployeeRefUpdate.countryCode2))
                    EmployeeRefUpdate.countryName2 = ListCboCountry?.FirstOrDefault(m => m.code == EmployeeRefUpdate.countryCode2)?.name;
                if (!string.IsNullOrEmpty(EmployeeRefUpdate.provinceCode2))
                    EmployeeRefUpdate.provinceName2 = ListCboProvince2?.FirstOrDefault(m => m.code == EmployeeRefUpdate.provinceCode2)?.name;
                if (!string.IsNullOrEmpty(EmployeeRefUpdate.districtCode2))
                    EmployeeRefUpdate.districtName2 = ListCboDistrict2?.FirstOrDefault(m => m.code == EmployeeRefUpdate.districtCode2)?.name;
                if (!string.IsNullOrEmpty(EmployeeRefUpdate.wardCode2))
                    EmployeeRefUpdate.wardName2 = ListCboDistrict2?.FirstOrDefault(m => m.code == EmployeeRefUpdate.wardCode2)?.name;

                EmployeeRefUpdate.placeOfResidence = $"{EmployeeRefUpdate.houseNumber1?.Trim()} " +
                    $"{EmployeeRefUpdate.wardName1?.Trim()} {EmployeeRefUpdate.districtName1?.Trim()} " +
                    $"{EmployeeRefUpdate.provinceName1?.Trim()} {EmployeeRefUpdate.countryName1?.Trim()}";
                EmployeeRefUpdate.placeOfResidence = EmployeeRefUpdate.placeOfResidence?.Trim();

                EmployeeRefUpdate.temporaryAddress = $"{EmployeeRefUpdate.houseNumber2?.Trim()} " +
                    $"{EmployeeRefUpdate.wardName2?.Trim()} {EmployeeRefUpdate.districtName2?.Trim()} " +
                    $"{EmployeeRefUpdate.provinceName2?.Trim()} {EmployeeRefUpdate.countryName2?.Trim()}";
                EmployeeRefUpdate.temporaryAddress = EmployeeRefUpdate.temporaryAddress?.Trim();
                EmployeeRefUpdate.userSign = UserId;
                EmployeeRefUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(EmployeeRefUpdate);
                int result = await _personnelService.UpdateEmployeeAsync(processKey, UserId, Token, content);
                if (result > 0) await showVoucher();
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
        /// Thay đổi giá trị combobox
        /// </summary>
        /// <param name="value"></param>
        /// <param name="controlID"></param>
        /// <returns></returns>
        protected async Task ComboboxValueChangedHandler(object? value
            , string controlID = nameof(EmployeeRefUpdate.countryCode1))
        {
            try
            {
                if (firstRender) return;
                switch (controlID)
                {
                    // chọn dữ liệu ở Hộ khẩu thường trú
                    case nameof(EmployeeRefUpdate.countryCode1):
                        await ShowLoading();
                        await Task.Delay(75);
                        ListCboProvince1 = await getProvince($"{value}");
                        ListCboDistrict1 = new List<ComboboxModel>();
                        ListCboWard1 = new List<ComboboxModel>();
                        EmployeeRefUpdate.countryCode1 = $"{value}";
                        EmployeeRefUpdate.provinceCode1 = string.Empty;
                        EmployeeRefUpdate.districtCode1 = string.Empty;
                        EmployeeRefUpdate.wardCode1 = string.Empty;
                        EmployeeRefUpdate.placeOfResidence = string.Empty;
                        break;
                    case nameof(EmployeeRefUpdate.provinceCode1):
                        await ShowLoading();
                        await Task.Delay(75);
                        ListCboDistrict1 = await getDistrict($"{value}");
                        ListCboWard1 = new List<ComboboxModel>();
                        EmployeeRefUpdate.provinceCode1 = $"{value}";
                        EmployeeRefUpdate.districtCode1 = string.Empty;
                        EmployeeRefUpdate.wardCode1 = string.Empty;
                        EmployeeRefUpdate.placeOfResidence = string.Empty;
                        break;
                    case nameof(EmployeeRefUpdate.districtCode1):
                        await ShowLoading();
                        await Task.Delay(75);
                        ListCboWard1 = await getWard($"{EmployeeRefUpdate.provinceCode1}", $"{value}");
                        EmployeeRefUpdate.districtCode1 = $"{value}";
                        EmployeeRefUpdate.wardCode1 = string.Empty;
                        EmployeeRefUpdate.placeOfResidence = string.Empty;
                        break;
                    // Chỗ ở hiện nay
                    case nameof(EmployeeRefUpdate.countryCode2):
                        await ShowLoading();
                        await Task.Delay(75);
                        ListCboProvince2 = await getProvince($"{value}");
                        ListCboDistrict2 = new List<ComboboxModel>();
                        ListCboWard2 = new List<ComboboxModel>();
                        EmployeeRefUpdate.countryCode2 = $"{value}";
                        EmployeeRefUpdate.provinceCode2 = string.Empty;
                        EmployeeRefUpdate.districtCode2 = string.Empty;
                        EmployeeRefUpdate.wardCode2 = string.Empty;
                        EmployeeRefUpdate.temporaryAddress = string.Empty;
                        break;
                    case nameof(EmployeeRefUpdate.provinceCode2):
                        await ShowLoading();
                        await Task.Delay(75);
                        ListCboDistrict2 = await getDistrict($"{value}");
                        ListCboWard2 = new List<ComboboxModel>();
                        EmployeeRefUpdate.provinceCode2 = $"{value}";
                        EmployeeRefUpdate.districtCode2 = string.Empty;
                        EmployeeRefUpdate.wardCode2 = string.Empty;
                        EmployeeRefUpdate.temporaryAddress = string.Empty;
                        break;
                    case nameof(EmployeeRefUpdate.districtCode2):
                        await ShowLoading();
                        await Task.Delay(75);
                        ListCboWard2 = await getWard($"{EmployeeRefUpdate.provinceCode2}", $"{value}");
                        EmployeeRefUpdate.districtCode2 = $"{value}";
                        EmployeeRefUpdate.wardCode2 = string.Empty;
                        EmployeeRefUpdate.temporaryAddress = string.Empty;
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
                EmployeeRefUpdate.isEqualsHousehold = value;
                if (!value) return;
                ListCboProvince2 = ListCboProvince1;
                ListCboDistrict2 = ListCboDistrict1;
                ListCboWard2 = ListCboWard1;
                EmployeeRefUpdate.countryCode2 = EmployeeRefUpdate.countryCode1;
                EmployeeRefUpdate.provinceCode2 = EmployeeRefUpdate.provinceCode1;
                EmployeeRefUpdate.districtCode2 = EmployeeRefUpdate.districtCode1;
                EmployeeRefUpdate.wardCode2 = EmployeeRefUpdate.wardCode1;
                EmployeeRefUpdate.houseNumber2 = EmployeeRefUpdate.houseNumber1;
                EmployeeRefUpdate.temporaryAddress = EmployeeRefUpdate.placeOfResidence;
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
        
        protected async Task ChangePassHandler()
        {
            try
            {

            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ChangePassHandler");
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
