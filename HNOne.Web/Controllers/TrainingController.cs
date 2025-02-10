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
        const string YCDT_NB = "Nội bộ";
        const string STRING_KEY_EVENT_POST = "TRAINING_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "TRAINING_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_PUT_EVALUETE = "TRAINING_CONTROLLER_PUT_EVALUETE";
        const string STRING_KEY_EVENT_DELETE = "TRAINING_CONTROLLER_DELETE";
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
        public GridSelectionMode DxGridEmployeeSelectionMode { get; set; } = GridSelectionMode.Single; // chọn môt/nhiều
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn
        public IReadOnlyList<object>? ListEmpSelected { get; set; } // danh sách nhân viên được chọn
        public bool firstRender = true;
        public string? VoucherHistory { get; set; } = string.Empty; // lịch sử chứng từ
        // lock control lại
        public bool IsReadonlyControl { get; set; } = false;

        // nút quyền
        public bool IsAllowPost { get; set; }
        public bool IsAllowDelete { get; set; }
        public bool IsAllowPut { get; set; }
        public bool IsAllowPutEvaluete { get; set; }
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-dao-tao");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    this.firstRender = firstRender;
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Đào tạo"),
                        new BreadcrumbModel("Đào tạo", "danh-sach-dao-tao"),
                        new BreadcrumbModel("Chi tiết đào tạo", isActive: true),
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
        /// <summary>
        /// kiểm tra quyền nút
        /// </summary>
        /// <returns></returns>
        private async Task checkPermission(string menuId)
        {
            List<string> lstKey = await CheckEventPermission(menuId);
            IsAllowPost = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_POST) != null;
            IsAllowDelete = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_DELETE) != null;
            IsAllowPut = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_PUT) != null;
            IsAllowPutEvaluete = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_PUT_EVALUETE) != null;
        }

        private void initDataAsync(bool isRefresh = false)
        {
            // GÁN DỮ LIỆU MẶC ĐỊNH
            TrainDocument.statusCode = CommonConstants.STATUS_CODE_ADD; // mặc định là chờ xử lý
            TrainDocument.typeOfTraning = YCDT_NB;
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
                var getTask6 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.HinhThucDaoTao)); // ds trạng thái
                await Task.WhenAll(
                    getTask5,
                    getTask6
                );
                ListCboStatus = await getTask5;
                ListCboTrainFormat = await getTask6;
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
            if (string.IsNullOrWhiteSpace(TrainDocument.typeOfTraning))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Y/c đào tạo");
                fieldName = "typeOfTraning";
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
                        // chỗ chọn nhân viên lập
                        DxGridEmployeeSelectionMode = GridSelectionMode.Single;
                        IsShowDialogEmpSearch = true;
                        break;
                    case nameof(ListEmpSelected):
                        DxGridEmployeeSelectionMode = GridSelectionMode.Multiple;
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
                switch (pPopupType)
                {
                    case nameof(TrainDocument.employeeSignatureCode):
                        EmployeeModel employee = (EmployeeModel)EmployeeSelected;
                        TrainDocument.employeeSignatureId = employee.id;
                        TrainDocument.employeeSignatureCode = employee.code;
                        TrainDocument.employeeSignatureName = employee.name;
                        IsShowDialogEmpSearch = false;
                        break;
                    case nameof(ListEmpSelected):
                        if (ListEmpSelected.IsNullOrEmpty()) break;
                        ListOfTrainings ??= new List<Training1Model>();
                        foreach (var item in ListEmpSelected!.Cast<EmployeeModel>())
                        {
                            if (ListOfTrainings.Any(m => m.employeeCode == item.code)) continue;
                            var otraining1 = new Training1Model();
                            otraining1.employeeId = item.id;
                            otraining1.employeeCode = item.code;
                            otraining1.employeeName = item.name;
                            ListOfTrainings.Add(otraining1);
                        }
                        GridOfTrainings?.Reload();
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
        protected void EventCallbackEmpListChangedHandler(IReadOnlyList<object>? lstEmp) => ListEmpSelected = lstEmp;

        protected async Task SaveDataHandler()
        {
            try
            {
                await checkPermission(MenuId);
                if ((pActionType == nameof(EnumType.Add) && !IsAllowPost) || (pActionType != nameof(EnumType.Add) && !IsAllowPut))
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
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
        /// lưu thông tin đánh giá nhân viên
        /// </summary>
        /// <returns></returns>
        protected async Task SaveDataEvalueteHandler()
        {
            try
            {
                await checkPermission(MenuId);
                if (!IsAllowPutEvaluete)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
                bool isConfirm = true;
                string errorMessage = string.Format(MessageConstants.MESSAGE_CONFIRM_UPDATE_FORMAT, "thông tin đánh giá");
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = ProcessConstants.PUT_TRAINING_EVALUATE;
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
                _logger.LogError(ex, "SaveDataEvalueteHandler");
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
                approval.employeeId = EmployeeId;
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

        protected void GridEditSavingHandler(GridEditModelSavingEventArgs e)
        {
            try
            {
                var itemEdit = (Training1Model)e.EditModel;
                var itemFind = ListOfTrainings?.FirstOrDefault(m => m.employeeId == itemEdit.employeeId && m.id == itemEdit.id);
                if (itemFind == null) return;
                itemFind.isAbsent = itemEdit.isAbsent;
                itemFind.noteForAll = itemEdit.noteForAll;
                itemFind.remark = itemEdit.remark;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GridEditSavingHandler");
            }
        }

        protected void DeleteDataHandler()
        {
            try
            {
                if (ListOfTrainings.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                var lstSelected = GridOfTrainings!.SelectedDataItems;
                if (lstSelected.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                foreach (Training1Model item in lstSelected) ListOfTrainings!.Remove(item);
                GridOfTrainings?.Reload();
                InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "DeleteDataHandler");
                ShowError(ex.Message);
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
                _navigationManager.NavigateTo($"/dao-tao?key={key}");
                TrainDocument = new TrainingModel();
                ListOfTrainings = new List<Training1Model>();
                pActionType = nameof(EnumType.Add);
                pDocEntry = -1;
                VoucherHistory = string.Empty;
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
        #endregion
    }
}
