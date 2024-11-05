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

        public string AddressStr = "{0}, {1}, {2}, {3}, {4}";

        public EmployeeModel EmployeeUpdate { get; set; } = new EmployeeModel();
        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public List<ComboboxModel>? ListCboPosition { get; set; } // cbo ds chức vụ
        public List<ComboboxModel>? ListCboTitle { get; set; } // cbo ds chức danh
        public List<EnumCatagoryModel>? ListCboMaritalStatus { get; set; } // cbo ds chức danh

        public List<ComboboxModel>? ListCboProvince { get; set; } // cbo ds tỉnh thành 


        public List<ComboboxModel>? ListCboCountry1 { get; set; } // cbo ds quốc gia
        public List<ComboboxModel>? ListCboProvince1 { get; set; } // cbo ds tỉnh thành 
        public List<ComboboxModel>? ListCboDistrict1 { get; set; } // cbo ds quận huyện 
        public List<ComboboxModel>? ListCboWard1 { get; set; } // cbo ds phường xã 

        public List<ComboboxModel>? ListCboCountry2 { get; set; } // cbo ds quốc gia
        public List<ComboboxModel>? ListCboProvince2 { get; set; } // cbo ds tỉnh thành 
        public List<ComboboxModel>? ListCboDistrict2 { get; set; } // cbo ds quận huyện 
        public List<ComboboxModel>? ListCboWard2 { get; set; } // cbo ds phường xã 


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

                //thường trú
                var getTask6 = _masterDataService.GetLocationData(UserId, Token, BranchId, nameof(EnumLocation.County)); // ds Quốc gia
                var getTask7 = _masterDataService.GetLocationData(UserId, Token, BranchId, nameof(EnumLocation.Province)); // ds tỉnh thành
                var getTask8 = _masterDataService.GetLocationData(UserId, Token, BranchId, nameof(EnumLocation.District), EmployeeUpdate.provinceCode1); // ds quận huyện thường trú
                var getTask9 = _masterDataService.GetLocationData(UserId, Token, BranchId, nameof(EnumLocation.Ward), EmployeeUpdate.provinceCode1, EmployeeUpdate.districtCode1); // ds xã phường thường trú

                // hiện nay
                var getTask10 = _masterDataService.GetLocationData(UserId, Token, BranchId, nameof(EnumLocation.County)); // ds Quốc gia
                var getTask11 = _masterDataService.GetLocationData(UserId, Token, BranchId, nameof(EnumLocation.Province)); // ds tỉnh thành
                var getTask12 = _masterDataService.GetLocationData(UserId, Token, BranchId, nameof(EnumLocation.District), EmployeeUpdate.provinceCode2); // ds quận huyện thường trú
                var getTask13 = _masterDataService.GetLocationData(UserId, Token, BranchId, nameof(EnumLocation.Ward), EmployeeUpdate.provinceCode2, EmployeeUpdate.districtCode2); // ds xã phường thường trú

                var getTask14 = _masterDataService.GetLocationData(UserId, Token, BranchId, nameof(EnumLocation.Province)); // ds tỉnh thành

                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask3,
                    getTask4,
                    getTask5,
                    getTask6,
                    getTask7,
                    getTask8,
                    getTask9,
                    getTask10,
                    getTask11,
                    getTask12,
                    getTask13,
                    getTask14
                );

                ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboPosition = (await getTask2)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboTitle = (await getTask3)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboMaritalStatus = await getTask5;
                ListCboStatus = (await getTask4)?.Where(m => m.rowOrder != 0).ToList();

                ListCboProvince = (await getTask14)?.Select(m => new ComboboxModel() { code = m.code, name = m.name })?.ToList();

                ListCboCountry1 = (await getTask6)?.Select(m => new ComboboxModel() { code = m.code, name = m.name })?.ToList();
                ListCboProvince1 = (await getTask7)?.Select(m => new ComboboxModel() { code = m.code, name = m.name })?.ToList();
                ListCboDistrict1 = (await getTask8)?.Select(m => new ComboboxModel() { code = m.code, name = m.name })?.ToList();
                ListCboWard1 = (await getTask9)?.Select(m => new ComboboxModel() { code = m.code, name = m.name })?.ToList();

                ListCboCountry2 = (await getTask10)?.Select(m => new ComboboxModel() { code = m.code, name = m.name })?.ToList();
                ListCboProvince2 = (await getTask11)?.Select(m => new ComboboxModel() { code = m.code, name = m.name })?.ToList();
                ListCboDistrict2 = (await getTask12)?.Select(m => new ComboboxModel() { code = m.code, name = m.name })?.ToList();
                ListCboWard2 = (await getTask13)?.Select(m => new ComboboxModel() { code = m.code, name = m.name })?.ToList();

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

        /// <summary>
        /// Lấy địa chỉ hiện nay
        /// </summary>
        private void getAddressStrNow()
        {
            EmployeeUpdate.countryName2 = ListCboCountry1?.FirstOrDefault(country => country.code == EmployeeUpdate?.countryCode2)?.name;
            EmployeeUpdate.provinceName2 = ListCboProvince1.FirstOrDefault(province => province.code == EmployeeUpdate.provinceCode2)?.name;
            EmployeeUpdate.districtName2 = ListCboDistrict1.FirstOrDefault(district => district.code == EmployeeUpdate.districtCode2)?.name;
            EmployeeUpdate.wardName2 = ListCboWard1.FirstOrDefault(ward => ward.code == EmployeeUpdate.wardCode2)?.name;

            EmployeeUpdate.temporaryAddress = (string.IsNullOrEmpty(EmployeeUpdate.houseNumber2) ? "" : ", " + EmployeeUpdate.houseNumber2) +
                                              (string.IsNullOrEmpty(EmployeeUpdate.wardName2) ? "" : ", " + EmployeeUpdate.wardName2) +
                                              (string.IsNullOrEmpty(EmployeeUpdate.districtName2) ? "" : ", " + EmployeeUpdate.districtName2) +
                                              (string.IsNullOrEmpty(EmployeeUpdate.provinceName2) ? "" : ", " + EmployeeUpdate.provinceName2) +
                                              (string.IsNullOrEmpty(EmployeeUpdate.countryName2) ? "" : ", " + EmployeeUpdate.countryName2);
            // Loại bỏ dấu phẩy thừa nếu có phần tử rỗng
            EmployeeUpdate.temporaryAddress = EmployeeUpdate.temporaryAddress.Trim().TrimStart(',');
            EmployeeUpdate.temporaryAddress = EmployeeUpdate.temporaryAddress.Trim().TrimEnd(',');
        }

        /// <summary>
        /// lấy địa chỉ tạm trú
        /// </summary>
        private void getAddressStrTemporary()
        {
            EmployeeUpdate.countryName1 = ListCboCountry2?.FirstOrDefault(country => country.code == EmployeeUpdate?.countryCode1)?.name;
            EmployeeUpdate.provinceName1 = ListCboProvince2.FirstOrDefault(province => province.code == EmployeeUpdate.provinceCode1)?.name;
            EmployeeUpdate.districtName1 = ListCboDistrict2.FirstOrDefault(district => district.code == EmployeeUpdate.districtCode1)?.name;
            EmployeeUpdate.wardName1 = ListCboWard2.FirstOrDefault(ward => ward.code == EmployeeUpdate.wardCode1)?.name;

            EmployeeUpdate.placeOfResidence = (string.IsNullOrEmpty(EmployeeUpdate.houseNumber1) ? "" : ", " + EmployeeUpdate.houseNumber1) +
                                              (string.IsNullOrEmpty(EmployeeUpdate.wardName1) ? "" : ", " + EmployeeUpdate.wardName1) +
                                              (string.IsNullOrEmpty(EmployeeUpdate.districtName1) ? "" : ", " + EmployeeUpdate.districtName1) +
                                              (string.IsNullOrEmpty(EmployeeUpdate.provinceName1) ? "" : ", " + EmployeeUpdate.provinceName1) +
                                              (string.IsNullOrEmpty(EmployeeUpdate.countryName1) ? "" : ", " + EmployeeUpdate.countryName1);
            // Loại bỏ dấu phẩy thừa nếu có phần tử rỗng
            EmployeeUpdate.placeOfResidence = EmployeeUpdate.placeOfResidence.Trim().TrimStart(',');
            EmployeeUpdate.placeOfResidence = EmployeeUpdate.placeOfResidence.Trim().TrimEnd(',');
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


        /// <summary>
        /// Quốc gia (chỗ ở thường trú)
        /// </summary>
        /// <param name="value"></param>
        protected void country1Changed(string value)
        {
            EmployeeUpdate.countryCode1 = value;
            getAddressStrTemporary();
        }
        /// <summary>
        /// Tỉnh thành (chỗ ở thường trú)
        /// </summary>
        /// <param name="value"></param>
        protected async void province1Changed(string value)
        {
            EmployeeUpdate.provinceCode1 = value;
            var getTask8 = _masterDataService.GetLocationData(UserId, Token, BranchId, nameof(EnumLocation.District), EmployeeUpdate.countryCode1,EmployeeUpdate.provinceCode1);
            await Task.WhenAll(
                    getTask8
                );

            ListCboDistrict1 = await getTask8;

            getAddressStrTemporary();
        }
        /// <summary>
        /// Quận huyện (chỗ ở thường trú)
        /// </summary>
        /// <param name="value"></param>
        protected void district1Changed(string value)
        {
            EmployeeUpdate.districtCode1 = value;
            getAddressStrTemporary();
        }
        /// <summary>
        /// Xã phường (chỗ ở thường trú)
        /// </summary>
        /// <param name="value"></param>
        protected void wardCode1Changed(string value)
        {
            EmployeeUpdate.wardCode1 = value;
            getAddressStrTemporary();
        }
        /// <summary>
        /// Số nhà đường phố (chỗ ở thường trú)
        /// </summary>
        /// <param name="value"></param>
        protected void HouseNumber1Changed(string value)
        {
            EmployeeUpdate.houseNumber1 = value;
            getAddressStrTemporary();
        }
        protected void CheckedChanged(bool value) // button Giống hộ khẩu thường trú?
        {
            if(value)
            {
                EmployeeUpdate.countryCode2 = EmployeeUpdate.countryCode1;
                EmployeeUpdate.provinceCode2 = EmployeeUpdate.provinceCode1;
                EmployeeUpdate.districtCode2 = EmployeeUpdate.districtCode1;
                EmployeeUpdate.wardCode2 = EmployeeUpdate.wardCode1;
                EmployeeUpdate.houseNumber2 = EmployeeUpdate.houseNumber1;
            }
            else if (!value)
            {
                EmployeeUpdate.countryCode2 = null;
                EmployeeUpdate.provinceCode2 = null;
                EmployeeUpdate.districtCode2 = null;
                EmployeeUpdate.wardCode2 = null;
                EmployeeUpdate.houseNumber2 = null;
            }
            getAddressStrNow();
        }
        /// <summary>
        /// Quốc gia (chỗ ở hiện nay)
        /// </summary>
        /// <param name="value"></param>
        protected void country2Changed(string value)
        {
            EmployeeUpdate.countryCode2 = value;
            getAddressStrNow();
        }
        /// <summary>
        /// Tỉnh thành (chỗ ở hiện nay)
        /// </summary>
        /// <param name="value"></param>
        protected void province2Changed(string value)
        {
            EmployeeUpdate.provinceCode2 = value;
            getAddressStrNow();
        }
        /// <summary>
        /// Quận huyện (chỗ ở hiện nay)
        /// </summary>
        /// <param name="value"></param>
        protected void district2Changed(string value)
        {
            EmployeeUpdate.districtCode2 = value;
            getAddressStrNow();
        }
        /// <summary>
        /// Xã phường (chỗ ở hiện nay)
        /// </summary>
        /// <param name="value"></param>
        protected void wardCode2Changed(string value) 
        {
            EmployeeUpdate.wardCode2 = value;
            getAddressStrNow();
        }
        /// <summary>
        /// Số nhà đường phố (chỗ ở hiện nay)
        /// </summary>
        /// <param name="value"></param>
        protected void HouseNumber2Changed(string value) 
        {
            EmployeeUpdate.houseNumber2 = value;
            getAddressStrNow();
        }
        #endregion
    }
}
