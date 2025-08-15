using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public partial class EmployeeController
    {
        #region Properties
        public bool IsCreatePopup { get; set; }
        public bool IsShowPopupInsurance { get; set; } // popup thêm mới Thông tin bảo hiểm
        public bool IsShowPopupFamily { get; set; } // popup thêm mới Thông tin quan hệ gia đình
        public bool IsShowPopupEducation { get; set; } // popup thêm mới Trình độ/Bằng cấp

        public List<InsuranceModel>? ListInsurance { get; set; } // danh sách thông tin lương
        public IGrid? GridInsurance { get; set; }
        public InsuranceModel InsuranceUpdate { get; set; } = new InsuranceModel();

        public List<FamilyRelationshipModel>? ListFamilyRelationship { get; set; } // danh sách quan hệ gia đình
        public IGrid? GridFamilyRelationship { get; set; }
        public FamilyRelationshipModel FamilyRelationshipUpdate { get; set; } = new FamilyRelationshipModel();

        public List<LevelOfEducationModel>? ListEducation { get; set; } // danh sách trình độ/bằng cấp
        public IGrid? GridEducation { get; set; }
        public LevelOfEducationModel LevelOfEducationUpdate { get; set; } = new LevelOfEducationModel();

        public List<InsuranceModel>? ListHistory { get; set; } // danh sách lịch sử công tác
        public IGrid? GridHistory { get; set; }

        public List<ContractModel>? ListContract { get; set; } // danh sách hợp đồng
        public IGrid? GridContract { get; set; }

        public List<EmployeeSalaryHistoryModel>? ListSalaryHistory { get; set; } // Lịch sử lương lấy theo hợp đồng
        public IGrid? GridSalaryHistory { get; set; }

        public List<DecisionDocumentModel>? ListWorkProgress { get; set; } // Diễn biến công tác, lấy theo quyết định
        public IGrid? GridWorkProgress { get; set; }

        public bool IsShowPopupActionHistory { get; set; }
        public AuditLogModel ActionLogSelected { get; set; } = new AuditLogModel();
        public List<AuditLogModel>? ListActionHistory { get; set; } // Lịch sử lương lấy theo hợp đồng
        public IGrid? GridActionHistory { get; set; }

        public List<EnumCatagoryModel>? ListCboInsuranceType { get; set; } // loại bảo hiểm
        public List<EnumCatagoryModel>? ListCboRank { get; set; } // cbo xếp loại
        private List<FileUploadModel>? lstFileTemp { get; set; } // danh sách file tạm
        #endregion

        #region Private Functions
        private void validateForSaveInsurance(ref string errorMessage, ref string fieldName)
        {
            if (EmployeeUpdate.id < 1)
            {
                errorMessage = "Vui lòng lưu thông tin trước khi thêm bảo hiểm";
                fieldName = "";
                return;
            }
            if (string.IsNullOrEmpty(InsuranceUpdate.insuranceType))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Loại bảo hiểm");
                fieldName = "txtInsuranceType";
                return;
            }
            if (string.IsNullOrEmpty(InsuranceUpdate.insuranceNo))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Số bảo hiểm");
                fieldName = "txtInsuranceNo";
                return;
            }
        }

        private void validateForSaveEducation(ref string errorMessage, ref string fieldName)
        {
            if (EmployeeUpdate.id < 1)
            {
                errorMessage = "Vui lòng lưu thông tin trước khi thêm bảo hiểm";
                fieldName = "";
                return;
            }
            if (string.IsNullOrEmpty(LevelOfEducationUpdate.levelOfEducation))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Trình độ đào tạo");
                fieldName = "txtLevelOfEducation";
                return;
            }
            if (string.IsNullOrEmpty(LevelOfEducationUpdate.educationalInstitution1))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Nơi đào tạo");
                fieldName = "txtEducationalInstitution1";
                return;
            }
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

        /// <summary>
        /// lấy danh sách hợp đồng
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
        /// lấy danh sách trình độ đại học
        /// </summary>
        /// <returns></returns>
        private async Task getEducation()
        {
            RequestModel request = new RequestModel();
            request.employeeId = EmployeeUpdate.id;
            request.userId = UserId;
            request.token = Token;
            request.branchId = BranchId;
            ListEducation = new List<LevelOfEducationModel>();
            ListEducation = await _personnelService.GetEducationAsync(request);
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
        /// Lấy danh sách lịch sử lương của nhân viên
        /// </summary>
        /// <returns></returns>
        private async Task getSalaryHistoryList()
        {
            var lstSalary = await _personnelService.GetSalaryHistoryAsync(UserId, Token, EmployeeUpdate.id, isShowToast: false);
            lstSalary = lstSalary?.Update(m => {
                if(m.contractId > 0)
                {
                    Dictionary<string, string> pParams = new Dictionary<string, string>
                    {
                        { "pActionType", nameof(EnumType.Update) },
                        { "pDocEntry", $"{m.contractId}" },
                    };
                    m.linkContract = _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
                }
                if (m.contractAppendixId > 0)
                {
                    Dictionary<string, string> pParams = new Dictionary<string, string>
                    {
                        { "pActionType", nameof(EnumType.Update) },
                        { "pDocEntry", $"{m.contractAppendixId}" },
                        { "pContractId", $"{m.contractId}" },
                    };
                    m.linkContractAppendix = _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
                }
            })?.ToList();
            ListSalaryHistory = lstSalary;
        }

        /// <summary>
        /// lấy lịch sử thao tác
        /// </summary>
        /// <returns></returns>
        private async Task getActionHistoryList()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.token = Token;
            request.branchId = BranchId;
            request.type = ProcessConstants.GET_COMBO_AUDIT_LOG_BY_EMPLOYEE;
            request.opt = EmployeeUpdate.id.ToString();
            var result = await _masterDataService.GetMasterDataAsync<AuditLogModel>(request);
            ListActionHistory = result;
        }
        
        /// <summary>
        /// Lấy diễn biến công tác theo quyết định
        /// </summary>
        /// <returns></returns>
        private async Task getWorkProgressList()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.branchId = BranchId;
            request.token = Token;
            request.opt = ActiveTabIndex == 0 ? "ACTIVE" : "";
            request.type = "BY_EMPLOYEE";
            request.employeeId = EmployeeUpdate.id;
            request.process = ProcessConstants.GET_DECISION_DOCUMENT;
            var lstWorkProgress = await _workforceService.GetDecisionDocumentAsync(request, isShowToast: false);
            lstWorkProgress = lstWorkProgress?.Update(m =>
            {
                Dictionary<string, string> pParams = new Dictionary<string, string>
                {
                    { "pActionType", nameof(EnumType.Update) },
                    { "pDocEntry", $"{m.id}" },
                };
                m.link = "chung-tu-quyet-dinh?key=" + _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
            })?.ToList();
            ListWorkProgress = lstWorkProgress;
        }
        #endregion

        #region Protected Functions

        /// <summary>
        /// Lưu thông tin hợp đồng
        /// </summary>
        /// <returns></returns>
        protected async Task SaveInsuranceHandler()
        {
            try
            {
                string errorMessage = string.Empty;
                string fieldName = string.Empty;
                validateForSaveInsurance(ref errorMessage, ref fieldName);
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
                // lưu hình ảnh
                if (!lstFileTemp.IsNullOrEmpty())
                {
                    var lstAllowedExtensions = AllowedExtensions.Split(",").Select(m => m?.Trim());
                    var lstFileExtension = lstFileTemp!.Select(m => Path.GetExtension(m.fileName));
                    var checkExist = lstFileExtension.Any(m => !lstAllowedExtensions.Contains(m));
                    if (checkExist)
                    {
                        _toastService.ShowWarning("Bạn chỉ được phép đính kèm tệp dạng hình ảnh, tài liệu");
                        return;
                    }
                    var lstImages = await _masterDataService.UploadImagesAsync(lstFileTemp!, "InsuranceController", enpoint: EnpointConstants.MASTERDATA_UPLOAD_FILE);
                    if (lstImages.IsNullOrEmpty()) return;
                    InsuranceUpdate.filePath = lstImages![0].filePath;
                    InsuranceUpdate.fileName = lstImages![0].fileName;
                    lstFileTemp = new List<FileUploadModel>();
                }
                string processKey = IsCreatePopup ? ProcessConstants.POST_INSURANCE : ProcessConstants.PUT_INSURANCE;
                InsuranceUpdate.insuranceTypeName = ListCboInsuranceType?.FirstOrDefault(m => m.code == InsuranceUpdate.insuranceType)?.name;
                InsuranceUpdate.employeeId = EmployeeUpdate.id;
                InsuranceUpdate.userSign = UserId;
                InsuranceUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(InsuranceUpdate);
                isConfirm = await _personnelService.UpdateInsuranceAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await geInsurance();
                    IsShowPopupInsurance = false;
                }    
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "SaveInsuranceHandler");
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

        /// <summary>
        /// Lưu thông tin trình độ bằng cấp
        /// </summary>
        /// <returns></returns>
        protected async Task SaveEducationHandler()
        {
            try
            {
                string errorMessage = string.Empty;
                string fieldName = string.Empty;
                validateForSaveEducation(ref errorMessage, ref fieldName);
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
                string processKey = IsCreatePopup ? ProcessConstants.POST_EDUCATION : ProcessConstants.PUT_EDUCATION;
                LevelOfEducationUpdate.rankingName = ListCboRank?.FirstOrDefault(m => m.code == LevelOfEducationUpdate.rankingCode)?.name;
                LevelOfEducationUpdate.employeeId = EmployeeUpdate.id;
                LevelOfEducationUpdate.userSign = UserId;
                LevelOfEducationUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(LevelOfEducationUpdate);
                isConfirm = await _personnelService.UpdateInsuranceAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getEducation();
                    IsShowPopupEducation = false;
                }
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "SaveEducationHandler");
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
        /// làm mới dữ liệu
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        protected async Task RefreshHandler(string process = ProcessConstants.GET_EDUCATION)
        {
            try
            {
                await ShowLoading();
                await Task.Yield();
                switch(process)
                {
                    case nameof(IsShowPopupEducation):
                        await getEducation();
                        break;
                    case nameof(IsShowPopupInsurance):
                        await getEducation();
                        break;
                    case nameof(IsShowPopupFamily):
                        await getFamilyRelationship();
                        break;
                    case nameof(EnumObjType.Contracts):
                        await getContractList();
                        break;
                    case nameof(EmployeeSalaryHistoryModel):
                        await getSalaryHistoryList();
                        break;
                    case nameof(EnumObjType.DecisionDocuments):
                        await getWorkProgressList();
                        break;
                    case nameof(AuditLogModel):
                        await getActionHistoryList();
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
        
        /// <summary>
        /// load dữ liệu file để lưu hợp đồng
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected async Task OnLoadFileInsuranceHandler(InputFileChangeEventArgs args)
        {
            try
            {
                if (args.FileCount <= 0) return;
                var lstAllowedExtensions = AllowedExtensions.Split(",").Select(m => m?.Trim());
                var lstFileExtension = args.GetMultipleFiles().Select(m => Path.GetExtension(m.Name));
                var checkExist = lstFileExtension.Any(m => !lstAllowedExtensions.Contains(m));
                if (checkExist)
                {
                    _toastService.ShowWarning("Bạn chỉ được phép đính kèm tệp dạng hình ảnh, tài liệu");
                    return;
                }
                await ShowLoading();
                lstFileTemp ??= new List<FileUploadModel>();
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
                itemFile.isDelete = false;
                // cập nhật nó là true để khỏi upload -> nhưng phải remove temp. ví dụ họ chọn mà không lưu
                // lấy file chọn mới nhất. xóa mấy cái chọn cũ đi
                foreach (var item in lstFileTemp)
                {
                    item.isDelete = true;
                }
                lstFileTemp.Add(itemFile);
                InsuranceUpdate.fileName = file.Name;
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "OnLoadFileInsuranceHandler");
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
        /// download file đính kèm
        /// </summary>
        /// <param name="pItemDetails"></param>
        /// <returns></returns>
        protected async Task DownLoadFileHandler(InsuranceModel pItemDetails)
        {
            try
            {
                if (string.IsNullOrEmpty(pItemDetails.fileName)) return;
                string apiUrl = _configuration.GetSection("appSettings:ImageUrl").Value + "";
                var fileViewUrl = $"{apiUrl}InsuranceController/{pItemDetails.fileName}";
                await _jsRuntime.InvokeVoidAsync("open", fileViewUrl, "_blank");
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "OnLoadFileInsuranceHandler");
                _toastService!.ShowError(ex.Message);
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
