using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
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

        public List<EnumCatagoryModel>? ListCboInsuranceType { get; set; } // loại bảo hiểm
        public List<EnumCatagoryModel>? ListCboRank { get; set; } // cbo xếp loại
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
        #endregion
    }
}
