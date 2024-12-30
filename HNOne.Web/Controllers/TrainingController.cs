using Microsoft.AspNetCore.Components;
using HNOne.Web.Services.Interfaces;
using HNOne.Web.Components.Controls;
using Microsoft.JSInterop;
using HNOne.Web.Commons;
using HNOne.Model.Models;
using HNOne.Model;
using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Web.Models;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using HNOne.Web.Services;

namespace HNOne.Web.Controllers
{
    public class TrainingController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IApprovalService _approvalService { get; init; }
        [Inject] ITrainingService _trainingService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public int ActiveTabIndex { get; set; } = 0;
        public TrainingModel TrainDocument { get; set; } = new TrainingModel();
        public List<Training1Model>? ListOfTrainings { get; set; } // danh sách thông tin trong koas đạo tạo
        public IGrid? GridOfTrainings { get; set; }

        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public List<EnumCatagoryModel>? ListCboTrainFormat { get; set; } // cbo ds hình thức đào tạo
        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public bool IsShowDialogEmpSearch { get; set; }
        public string? DepartmentIds { get; set; }
        public string? StatusIds { get; set; } // Tình trạng nào
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn
        public bool firstRender = true;
        public string? VoucherHistory { get; set; } = string.Empty; // lịch sử chứng từ
        // lock control lại
        public bool IsReadonlyControl { get; set; } = false;
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    //string errMessage = await CheckMenuPermissionAsync("danh-sach-hop-dong");
                    //if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    this.firstRender = firstRender;
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Đào tạo"),
                        new BreadcrumbModel("Chứng từ đề nghị"),
                        new BreadcrumbModel("Đề nghị nghỉ phép", "danh-sach-de-nghi-nghi-phep"),
                        new BreadcrumbModel("Chi tiết đề nghị nghỉ phép", isActive: true),
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    //
                    initDataAsync();
                    await buildComboAsync();
                    if (pDocEntry > 0)
                    {
                        await showVoucher();
                    }

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
            // GÁN DỮ LIỆU MẶC ĐỊNH
            TrainDocument.statusCode = CommonConstants.STATUS_CODE_ADD; // mặc định là chờ xử lý
            var uri = _navigationManager?.ToAbsoluteUri(_navigationManager.Uri);
            if (!isRefresh && uri != null && QueryHelpers.ParseQuery(uri.Query).Count > 0)
            {
                string key = uri.Query.Substring(5); // bỏ ?key=
                Dictionary<string, string> pParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(_encryptHelper.Decrypt(key))!;
                if (pParams != null && pParams.Any())
                {
                    if (pParams.ContainsKey("pActionType")) pActionType = Convert.ToString(pParams["pActionType"]);
                    if (pParams.ContainsKey("pDocEntry")) pDocEntry = Convert.ToInt32(pParams["pDocEntry"]);
                }
            }
            IsReadonlyControl = pActionType == nameof(EnumType.Update);
        }

        private async Task buildComboAsync()
        {
            try
            {
                var getTask5 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiHopDong)); // ds trạng thái
                await Task.WhenAll(
                    getTask5
                );
                ListCboStatus = await getTask5;
            }
            catch (Exception) { throw; }
        }

        private async Task showVoucher()
        {
            try
            {
                RequestModel request = new RequestModel();
                request.documentId = pDocEntry;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.process = ProcessConstants.GET_TRAINING;
                var task1 = _trainingService.GetMasterDataAsync<TrainingModel>(request);
                var task2 = getDocumentHistory();
                await Task.WhenAll(task1, task2);
                List<TrainingModel>? lstData = await task1;
                if (!lstData.IsNullOrEmpty())
                {
                    TrainDocument = lstData![0];
                    //cho phép chỉnh sữa khi tình trạng là: A (Tạo mới), Y (Đã gửi yêu cầu phê duyệt)
                    IsReadonlyControl = TrainDocument.statusCode != CommonConstants.STATUS_CODE_ADD
                        && TrainDocument.statusCode != CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                    if (!string.IsNullOrEmpty(TrainDocument.jsonDetail))
                    {
                        ListOfTrainings = JsonConvert.DeserializeObject<List<Training1Model>>(TrainDocument.jsonDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// lấy lịch sử chứng từ
        /// </summary>
        /// <returns></returns>
        private async Task getDocumentHistory()
            => VoucherHistory = await _approvalService.GetFunDocumentHistoryAsync(UserId, BranchId, Token, nameof(EnumObjType.Trainings), pDocEntry);

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (ListOfTrainings.IsNullOrEmpty())
            {
                errorMessage = "Vui lòng chọn nhân viên đào tạo!!!";
                fieldName = "gridInfo";
                return;
            }
            if (string.IsNullOrWhiteSpace(TrainDocument.trainingCourseName?.Trim()))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Khóa đào tạo");
                fieldName = "trainingCourseName";
                return;
            }
            if (string.IsNullOrWhiteSpace(TrainDocument.traningFormatCode))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Hình thức");
                fieldName = "traningFormatCode";
                return;
            }
            if (TrainDocument.employeeSignatureId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(TrainDocument.employeeSignatureId);
                return;
            }
            if (TrainDocument.fromDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Từ ngày");
                fieldName = "startDate";
                return;
            }
            if (TrainDocument.toDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Đến ngày");
                fieldName = "endDate";
                return;
            }
            if (TrainDocument.toDate.Value.Date < TrainDocument.fromDate.Value.Date)
            {
                errorMessage = MessageConstants.MESSAGE_FROM_DATE_TO_DATE_INVALID;
                fieldName = "startDate";
                return;
            }
        }

        /// <summary>
        /// kiểm tra dữ liệu trươc khi gửi phê duyệt
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="fieldName"></param>
        private void validateForSaveApproval(ref string errorMessage, ref string fieldName)
        {
            if (TrainDocument.id < 1)
            {
                errorMessage = "Vui lòng lưu thông tin chứng từ trước khi gửi phê duyệt";
                fieldName = "zzzz";
                return;
            }
            if (TrainDocument.employeeSignatureId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(TrainDocument.employeeSignatureId);
                return;
            }
        }
        #endregion

        #region Protected Functions
        protected async Task OpenPopupHandler(string type = nameof(EmployeeSelected),
            string popupType = nameof(TrainDocument.employeeSignatureCode))
        {
            try
            {
                pPopupType = popupType;
                switch (type)
                {
                    case nameof(EmployeeSelected):
                        //ListCboDepartment ??= new();
                        //DepartmentIds = string.Join(",", ListCboDepartment.Select(m => m.id));
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
                    case nameof(TrainDocument.employeeSignatureCode):
                        TrainDocument.employeeSignatureId = employee.id;
                        TrainDocument.employeeSignatureCode = employee.code;
                        TrainDocument.employeeSignatureName = employee.name;
                        IsShowDialogEmpSearch = false;
                        break;
                    case nameof(ListOfTrainings):
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
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = pActionType == nameof(EnumType.Add) ? ProcessConstants.POST_TRAINING : ProcessConstants.PUT_TRAINING;
                TrainDocument.branchId = BranchId;
                TrainDocument.userSign = UserId;
                TrainDocument.userSign2 = UserId;
                string json = JsonConvert.SerializeObject(TrainDocument);
                string jsonDetail = JsonConvert.SerializeObject(ListOfTrainings);
                int result = await _trainingService.UpdateTrainingAsync(processKey, UserId, Token, BranchId, json, jsonDetail);
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
        /// gửi phê duyệt
        /// </summary>
        /// <returns></returns>
        protected async Task SubmitForApprovalHandler()
        {
            try
            {
                string errorMessage = string.Empty;
                string fieldName = string.Empty; // trả ra trường nào cần validate
                bool isConfirm = true;
                validateForSaveApproval(ref errorMessage, ref fieldName);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                    return;
                }
                await Task.Yield();
                errorMessage = string.Format(MessageConstants.MESSAGE_CONFIRM_SEND_APPROVAL_FORMAT, $"đến nhân viên {TrainDocument.employeeSignatureName}");
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{errorMessage}");
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = ProcessConstants.POST_APPROVAL;
                ApprovalModel approval = new ApprovalModel();
                approval.docEntry = TrainDocument.id;
                approval.objType = nameof(EnumObjType.Trainings);
                approval.branchId = BranchId;
                approval.statusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                approval.userSign = UserId;
                approval.employeeSignatureId = TrainDocument.employeeSignatureId;
                string content = JsonConvert.SerializeObject(approval);
                isConfirm = await _approvalService.UpdateApprovalAsync(processKey, UserId, Token, json: content);
                if (isConfirm) await showVoucher();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "SubmitForApprovalHandler");
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
