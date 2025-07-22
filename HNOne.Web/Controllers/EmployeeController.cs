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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace HNOne.Web.Controllers
{
    public partial class EmployeeController : DocumentControllerBase
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
        public string AddressStr = "{0}, {1}, {2}, {3}, {4}";

        public EmployeeModel EmployeeUpdate { get; set; } = new EmployeeModel();
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public List<ComboboxModel>? ListCboWorkingBranch { get; set; } // cbo ds chi nhánh làm việc
        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public List<ComboboxModel>? ListCboPosition { get; set; } // cbo ds chức vụ
        public List<ComboboxModel>? ListCboTitle { get; set; } // cbo ds chức danh
        public List<ComboboxModel>? ListCboSubDepartment { get; set; } // cbo ds bộ phận
        public List<EnumCatagoryModel>? ListCboMaritalStatus { get; set; } // cbo ds tình trạng hộn nhân
        public List<EnumCatagoryModel>? ListCboRelationship { get; set; } // cbo ds quan hệ
        public List<EnumCatagoryModel>? ListCboEmployeeType { get; set; } // cbo ds loại nhân viên
        public List<ComboboxModel>? ListCboCountry { get; set; } // cbo quốc gia
        public List<ComboboxModel>? ListCboProvince { get; set; } // cbo ds tỉnh thành trong thông tin liên hệ

        public List<ComboboxModel>? ListCboProvince1 { get; set; } // cbo ds tỉnh thành 
        public List<ComboboxModel>? ListCboDistrict1 { get; set; } // cbo ds quận huyện 
        public List<ComboboxModel>? ListCboWard1 { get; set; } // cbo ds phường xã 

        public List<ComboboxModel>? ListCboProvince2 { get; set; } // cbo ds tỉnh thành 
        public List<ComboboxModel>? ListCboDistrict2 { get; set; } // cbo ds quận huyện 
        public List<ComboboxModel>? ListCboWard2 { get; set; } // cbo ds phường xã 
        public List<EnumCatagoryModel>? ListCboShift { get; set; } // cbo ds ca làm việc


        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public bool IsShowDialogEmpSearch { get; set; }
        public string? StatusIds { get; set; } // Tình trạng nào
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn

        private List<FileUploadModel>? lstImageTemp { get; set; } // danh sách ảnh tạm
        //public bool IsReadonlyControl { get; set; } = false;
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if(firstRender)
            {
                try
                {
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-nhan-vien");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    this.firstRender = firstRender;
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Nhân sự"),
                        new BreadcrumbModel("Danh sách nhân viên", enpoint: "/danh-sach-nhan-vien"),
                        new BreadcrumbModel("Hồ sơ nhân viên", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);

                    //
                    initDataAsync();
                    await buildComboAsync();
                    if (pDocEntry > 0) await showVoucher();
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

        private void initDataAsync(bool isRefresh = false)
        {
            EmployeeUpdate.branchId = BranchId;
            var uri = _navigationManager?.ToAbsoluteUri(_navigationManager.Uri);
            if(!isRefresh && uri != null && QueryHelpers.ParseQuery(uri.Query).Count > 0)
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
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.token = Token;
                request.branchId = BranchId;
                request.opt = CommonConstants.ENUM_ACTIVE;
                request.process = ProcessConstants.GET_WORKING_BRANCH;
                var getTask1 = _masterDataService.GetDepartmentAsync(UserId, Token, BranchId, opt: CommonConstants.ENUM_ACTIVE); // ds phòng ban
                var getTask2 = _masterDataService.GetPositionAsync(UserId, Token, BranchId, opt: CommonConstants.ENUM_ACTIVE); // ds chức vụ
                var getTask4 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiNhanVien)); // ds trạng thái
                var getTask5 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.TinhTrangHonNhan)); // ds tình trạng hôn nhân
                var getTask6 = _masterDataService.GetLocationAsync(UserId, Token, nameof(EnumCatagory.County)); // ds trạng thái
                var getTask7 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.QuanHeGiaDinh)); // ds quan hệ gia đình
                var getTask9 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.LoaiNhanVien)); // ds loại nhân viên
                var getTask8 = _masterDataService.GetLocationAsync(UserId, Token, nameof(EnumCatagory.Province), "VN");
                var getTask10 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.CaLamViec)); // ds loại nhân viên
                var getTask11 = _masterDataService.GetBranchAsync(1, "", opt: CommonConstants.ENUM_PAGE_LOGIN); // danh sách chi nhánh
                var getTask12 = _masterDataService.GetMasterAsync<WorkingBranchModel>(request, isShowToast: false); // danh sách chi nhánh
                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask4,
                    getTask5,
                    getTask6,
                    getTask7,
                    getTask8,
                    getTask9,
                    getTask10,
                    getTask11,
                    getTask12
                );
                ListCboBranch = (await getTask11)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
                ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboPosition = (await getTask2)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboMaritalStatus = await getTask5;
                ListCboStatus = (await getTask4)?.Where(m => m.rowOrder != 0).ToList();
                if (!ListCboStatus.IsNullOrEmpty()) EmployeeUpdate.statusId = ListCboStatus![0].code;
                ListCboCountry = await getTask6;
                ListCboRelationship = await getTask7;
                ListCboProvince = await getTask8;
                ListCboEmployeeType = await getTask9;
                ListCboShift = await getTask10;
                ListCboWorkingBranch = (await getTask12)?.Select(m => new ComboboxModel() { id = m.id, name = m.name})?.ToList();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "buildComboAsync");
            }
        }

        /// <summary>
        /// lấy danh sách theo phòng ban
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        private async Task buildComboByDepartmentAsync(int departmentId)
        {
            try
            {
                var getTask3 = _masterDataService.GetTitleAsync(UserId, Token, BranchId, opt: CommonConstants.ENUM_ACTIVE); // ds chức danh
                var getTask13 = _masterDataService.GetSubDepartmentAsync(UserId, Token, BranchId, opt: CommonConstants.ENUM_ACTIVE); // ds bộ phận
                await Task.WhenAll(getTask3, getTask13);
                ListCboTitle = (await getTask3)?.Where(m=>m.departmentId == departmentId).Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboSubDepartment = (await getTask13)?.Where(m => m.departmentId == departmentId)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "buildComboByDepartmentAsync");
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
                    lstTask.Add(buildComboByDepartmentAsync(EmployeeUpdate.departmentId));
                    lstTask.Add(geInsurance()); // danh sách hợp đồng
                    lstTask.Add(getFamilyRelationship()); // danh sách quan hệ gia đình
                    lstTask.Add(getEducation()); // danh sách trình độ đại học
                    lstTask.Add(getContractList()); // danh sách hợp đồng
                    lstTask.Add(getSalaryHistoryList()); // lịch sử lương

                    await Task.WhenAll(lstTask);
                }
            }
            catch (Exception) { throw; }
        }

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if(string.IsNullOrEmpty(EmployeeUpdate.employeeType))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Loại nhân viên");
                fieldName = nameof(EmployeeUpdate.employeeType);
                return;
            }    
            if (string.IsNullOrEmpty(EmployeeUpdate.name))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Họ tên");
                fieldName = nameof(EmployeeUpdate.name);
                return;
            }
            if (string.IsNullOrEmpty(EmployeeUpdate.statusId))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Trạng thái");
                fieldName = nameof(EmployeeUpdate.statusId);
                return;
            }
            if (string.IsNullOrEmpty(EmployeeUpdate.gender))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Giới tính");
                fieldName = nameof(EmployeeUpdate.gender);
                return;
            }
            if (EmployeeUpdate.dateOfBirth == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ngày sinh");
                fieldName = nameof(EmployeeUpdate.dateOfBirth);
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
            if ((EmployeeUpdate.titleId ?? -1) < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chức danh");
                fieldName = nameof(EmployeeUpdate.titleId);
                return;
            }
            if ((EmployeeUpdate.subDepartmentId ?? -1) < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Bộ phận");
                fieldName = nameof(EmployeeUpdate.subDepartmentId);
                return;
            }
            if (EmployeeUpdate.workingBranchId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chi nhánh làm việc");
                fieldName = nameof(EmployeeUpdate.workingBranchId);
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
            //if (EmployeeUpdate.probationStartDate == null)
            //{
            //    errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ngày thử việc");
            //    fieldName = nameof(EmployeeUpdate.probationStartDate);
            //    return;
            //}
            //if (EmployeeUpdate.probationEndDate == null)
            //{
            //    errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Ngày kết thúc thử việc");
            //    fieldName = nameof(EmployeeUpdate.startDate);
            //    return;
            //}
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
        
        #endregion

        #region Projected
        /// <summary>
        /// hiển thị các popup
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected async Task OpenPopupHandler(string type = nameof(EmployeeSelected), string popupType = nameof(EmployeeUpdate.managerCode)
            , EnumType pAction = EnumType.Add, object? pItemDetails = null)
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
                    case nameof(IsShowPopupInsurance):
                        await ShowLoading();
                        await Task.Delay(75);
                        ListCboInsuranceType = await _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.LoaiBaoHiem));
                        if (pAction == EnumType.Add)
                        {
                            IsCreatePopup = true;
                            InsuranceUpdate = new InsuranceModel();
                        }
                        else
                        {
                            InsuranceModel insurance = JsonConvert.DeserializeObject<InsuranceModel>
                                    (JsonConvert.SerializeObject(pItemDetails))!;
                            InsuranceUpdate.id = insurance.id;
                            InsuranceUpdate.employeeId = insurance.employeeId;
                            InsuranceUpdate.insuranceType = insurance.insuranceType;
                            InsuranceUpdate.insuranceTypeName = insurance.insuranceTypeName;
                            InsuranceUpdate.insuranceNo = insurance.insuranceNo;
                            InsuranceUpdate.startDate = insurance.startDate;
                            InsuranceUpdate.endDate = insurance.endDate;
                            InsuranceUpdate.rate = insurance.rate;
                            InsuranceUpdate.zipCode = insurance.zipCode;
                            InsuranceUpdate.address = insurance.address;
                            InsuranceUpdate.addressNo = insurance.addressNo;
                            IsCreatePopup = false;
                        }
                        IsShowPopupInsurance = true;
                        break;
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
                            FamilyRelationshipUpdate.isDeduction = family.isDeduction;
                            FamilyRelationshipUpdate.fromDate = family.fromDate;
                            FamilyRelationshipUpdate.toDate = family.toDate;
                            IsCreatePopup = false;
                        }
                        IsShowPopupFamily = true;
                        break;
                    case nameof(IsShowPopupEducation):
                        await ShowLoading();
                        await Task.Delay(75);
                        ListCboRank = await _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.XepLoaiDaoTao));
                        if (pAction == EnumType.Add)
                        {
                            IsCreatePopup = true;
                            LevelOfEducationUpdate = new LevelOfEducationModel();
                        }
                        else
                        {
                            LevelOfEducationModel education = JsonConvert.DeserializeObject<LevelOfEducationModel>
                                    (JsonConvert.SerializeObject(pItemDetails))!;
                            LevelOfEducationUpdate.id = education.id;
                            LevelOfEducationUpdate.employeeId = education.employeeId;
                            LevelOfEducationUpdate.fromYear = education.fromYear;
                            LevelOfEducationUpdate.toYear = education.toYear;
                            LevelOfEducationUpdate.levelOfEducation = education.levelOfEducation;
                            LevelOfEducationUpdate.educationalInstitution1 = education.educationalInstitution1;
                            LevelOfEducationUpdate.educationalInstitution2 = education.educationalInstitution2;
                            LevelOfEducationUpdate.majorCode = education.majorCode;
                            LevelOfEducationUpdate.rankingCode = education.rankingCode;
                            LevelOfEducationUpdate.rankingName = education.rankingName;
                            LevelOfEducationUpdate.isComplete = education.isComplete;
                            IsCreatePopup = false;
                        }
                        IsShowPopupEducation = true;
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
                    case nameof(EmployeeUpdate.managerCode2):
                        EmployeeUpdate.managerId2 = employee.id;
                        EmployeeUpdate.managerCode2 = employee.code;
                        EmployeeUpdate.managerName2 = employee.name;
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
        /// Lưu thông tin nhân viên
        /// </summary>
        /// <returns></returns>
        protected async Task SaveDataHandler(bool isCreateAccount = false)
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
                // nếu có tạo tài khoản luôn không?
                if (isCreateAccount)
                {
                    errorMessage = "Thao tác này sẽ phát sinh tài khoản đăng nhập cho nhân viên.<br />";
                }
                errorMessage += pActionType == nameof(EnumType.Add) ? MessageConstants.MESSAGE_CONFIRM_ADD : MessageConstants.MESSAGE_CONFIRM_UPDATE;
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                // lưu hình ảnh
                if (!lstImageTemp.IsNullOrEmpty())
                {
                    var lstImages = await _masterDataService!.UploadImagesAsync(lstImageTemp!, nameof(EmployeeController));
                    if (lstImages.IsNullOrEmpty()) return;
                    EmployeeUpdate.imageUrl = lstImages![0].fileName;
                }
                string processKey = pActionType == nameof(EnumType.Add) ? ProcessConstants.POST_EMPLOYEE : ProcessConstants.PUT_EMPLOYEE;
                if (!string.IsNullOrEmpty(EmployeeUpdate.provinceCode))
                    EmployeeUpdate.provinceName = ListCboProvince?.FirstOrDefault(m => m.code == EmployeeUpdate.provinceCode)?.name;
                // Lấy tên thông tin hộ khẩu thường trú
                if (!string.IsNullOrEmpty(EmployeeUpdate.countryCode1))
                    EmployeeUpdate.countryName1 = ListCboCountry?.FirstOrDefault(m => m.code == EmployeeUpdate.countryCode1)?.name;
                if (!string.IsNullOrEmpty(EmployeeUpdate.provinceCode1))
                    EmployeeUpdate.provinceName1 = ListCboProvince1?.FirstOrDefault(m => m.code == EmployeeUpdate.provinceCode1)?.name;
                if (!string.IsNullOrEmpty(EmployeeUpdate.districtCode1))
                    EmployeeUpdate.districtName1 = ListCboDistrict1?.FirstOrDefault(m => m.code == EmployeeUpdate.districtCode1)?.name;
                if (!string.IsNullOrEmpty(EmployeeUpdate.wardCode1))
                    EmployeeUpdate.wardName1 = ListCboDistrict1?.FirstOrDefault(m => m.code == EmployeeUpdate.wardCode1)?.name;

                // Lấy tên thông tin Chỗ ở hiện nay
                if (!string.IsNullOrEmpty(EmployeeUpdate.countryCode2))
                    EmployeeUpdate.countryName2 = ListCboCountry?.FirstOrDefault(m => m.code == EmployeeUpdate.countryCode2)?.name;
                if (!string.IsNullOrEmpty(EmployeeUpdate.provinceCode2))
                    EmployeeUpdate.provinceName2 = ListCboProvince2?.FirstOrDefault(m => m.code == EmployeeUpdate.provinceCode2)?.name;
                if (!string.IsNullOrEmpty(EmployeeUpdate.districtCode2))
                    EmployeeUpdate.districtName2 = ListCboDistrict2?.FirstOrDefault(m => m.code == EmployeeUpdate.districtCode2)?.name;
                if (!string.IsNullOrEmpty(EmployeeUpdate.wardCode2))
                    EmployeeUpdate.wardName2 = ListCboDistrict2?.FirstOrDefault(m => m.code == EmployeeUpdate.wardCode2)?.name;

                EmployeeUpdate.placeOfResidence = $"{EmployeeUpdate.houseNumber1?.Trim()} " +
                    $"{EmployeeUpdate.wardName1?.Trim()} {EmployeeUpdate.districtName1?.Trim()} " +
                    $"{EmployeeUpdate.provinceName1?.Trim()} {EmployeeUpdate.countryName1?.Trim()}";
                EmployeeUpdate.placeOfResidence = EmployeeUpdate.placeOfResidence?.Trim();

                EmployeeUpdate.temporaryAddress = $"{EmployeeUpdate.houseNumber2?.Trim()} " +
                    $"{EmployeeUpdate.wardName2?.Trim()} {EmployeeUpdate.districtName2?.Trim()} " +
                    $"{EmployeeUpdate.provinceName2?.Trim()} {EmployeeUpdate.countryName2?.Trim()}";
                EmployeeUpdate.temporaryAddress = EmployeeUpdate.temporaryAddress?.Trim();

                EmployeeUpdate.branchId = BranchId;
                EmployeeUpdate.userSign = UserId;
                EmployeeUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(EmployeeUpdate);
                int result = await _personnelService.UpdateEmployeeAsync(processKey, UserId, Token, content, isCreateAccount: isCreateAccount);
                if (result > 0)
                {
                    pActionType = nameof(EnumType.Update);
                    pDocEntry = result;
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
                    case nameof(EmployeeUpdate.departmentId):
                        await ShowLoading();
                        await Task.Delay(75);
                        int.TryParse($"{value}",out int departmentId);
                        await buildComboByDepartmentAsync(departmentId);
                        EmployeeUpdate.departmentId = departmentId;
                        EmployeeUpdate.titleId = -1;
                        EmployeeUpdate.subDepartmentId = -1;
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
                if (!value) return;
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
        
        /// <summary>
        /// Scroll đến section của element
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        protected async Task ScrollToSection(string sectionId)
        {
            try
            {
                //if (sectionId == "section-accordion-1" && ActiveTabIndex == 0) return;
                //if (sectionId == "section-accordion-2" && ActiveTabIndex == 1) return;
                await _jsRuntime.InvokeVoidAsync("scrollToSection", sectionId);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "ScrollToSection");
            }
        }

        /// <summary>
        /// load dữ liệu lên view
        /// </summary>
        /// <param name="args"></param>
        protected async void OnLoadFileHandler(InputFileChangeEventArgs args)
        {
            try
            {
                if (args.FileCount <= 0) return;
                await ShowLoading();
                lstImageTemp ??= new List<FileUploadModel>();
                var rootFolder = Path.Combine(_webHostEnvironment!.WebRootPath, "Upload", "Temps");
                //tạo thư mục
                if (!Directory.Exists(rootFolder)) Directory.CreateDirectory(rootFolder);
                string strFileFullName = string.Empty;
                var file = args.GetMultipleFiles().First();
                string fileNameNew = $"{Guid.NewGuid()}---{file.Name}";
                strFileFullName = Path.Combine(rootFolder, fileNameNew);
                await using FileStream fs = new(strFileFullName, FileMode.Create);
                await file.OpenReadStream(long.MaxValue).CopyToAsync(fs);
                await fs.FlushAsync();
                await fs.DisposeAsync();
                FileUploadModel itemFile = new FileUploadModel();
                itemFile.fileName = fileNameNew;
                itemFile.filePath = strFileFullName;
                itemFile.imageUrl = $"../Upload/Temps/{fileNameNew}";
                itemFile.isDelete = false;
                // cập nhật nó là true để khỏi upload -> nhưng phải remove temp. ví dụ họ chọn mà không lưu
                // lấy file chọn mới nhất. xóa mấy cái chọn củ đi
                foreach (var item in lstImageTemp)
                {
                    item.isDelete = true;
                }
                lstImageTemp.Add(itemFile);
                EmployeeUpdate.imageViewUrl = $"../Upload/Temps/{fileNameNew}";
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "OnLoadFileHandler");
                _toastService!.ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(75);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        
        /// <summary>
        /// tạo dữ liệu công làm việc của nhân viên
        /// </summary>
        /// <returns></returns>
        protected async Task GenerateWorkHandler()
        {
            try
            {
                if(EmployeeUpdate.id < 1) return;
                // muốn phát sinh công làm việc. phải kiểm tra dữ liệu công
                string errorMessage = string.Empty;
                bool isConfirm = true;
                errorMessage = $"Bạn có chắc muốn phát sinh dữ liệu công của kỳ [{DateTime.Now.Year}-{DateTime.Now.Month}] <br />" +
                    $"Cho nhân viên [{EmployeeUpdate.code}] không?";
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.POST_TIME_SHEET;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01);
                request.departmentIds = EmployeeUpdate.departmentId.ToString();
                request.employeeId = EmployeeUpdate.id;
                isConfirm = await _workforceService.UpdateMasterDataAsync(request);
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
        /// làm mới dữ liệu
        /// </summary>
        /// <returns></returns>
        protected async Task RefreshDataHandler()
        {
            try
            {
                await ShowLoading();
                Dictionary<string, string> pParams = new Dictionary<string, string>
                {
                    { "pActionType", $"{nameof(EnumType.Add)}" },
                    { "pDocEntry", $"{-1}" },
                };
                string key = _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams)); // mã hóa key
                _navigationManager.NavigateTo($"/ho-so-nhan-vien?key=?key={key}");
                EmployeeUpdate = new EmployeeModel();
                pActionType = nameof(EnumType.Add);
                pDocEntry = -1;
                ListInsurance = new List<InsuranceModel>();
                ListFamilyRelationship = new List<FamilyRelationshipModel>();
                ListEducation = new List<LevelOfEducationModel>();
                ListContract = new List<ContractModel>();
                initDataAsync(isRefresh: true);
                await buildComboAsync();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "RefreshDataHandler");
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        
        /// <summary>
        /// Tạo hợp đồng cho nhân viên
        /// </summary>
        /// <returns></returns>
        protected async Task CreateContractHandler()
        {
            try
            {
                _navigationManager.NavigateTo("/chi-tiet-hop-dong");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "CreateContractHandler");
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
