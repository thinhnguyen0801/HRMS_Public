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
using HNOne.Model.Entities;

namespace HNOne.Web.Controllers
{
    public class DecisionDocumentController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IApprovalService _approvalService { get; init; }
        [Inject] IWorkforceService _workforceService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        const string STRING_KEY_EVENT_POST = "DECISION_DOCUMENT_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "DECISION_DOCUMENT_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "DECISION_DOCUMENT_CONTROLLER_DELETE";
        const string STRING_KEY_EVENT_APPROVAL = "APPROVAL_CONTROLLER_PUT";

        public const string QD_NGHI_VIEC = "QD-001"; // quyết định nghỉ việc
        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public int ActiveTabIndex { get; set; } = 0;

        public DecisionDocumentModel RequestDocument { get; set; } = new DecisionDocumentModel();
        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public List<ComboboxModel>? ListCboReason { get; set; } // cbo ds lý do
        public List<EnumCatagoryModel>? ListCboDecisionType { get; set; } // cbo ds loại quyết định
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public List<ComboboxModel>? ListCboPosition { get; set; } // cbo ds chức vụ
        public List<ComboboxModel>? ListCboTitle { get; set; } // cbo ds chức danh
        public List<ComboboxModel>? ListCboSubDepartment { get; set; } // cbo ds bộ phận
        public List<ComboboxModel>? ListCboWorkingBranch { get; set; } // cbo ds chi nhánh làm việc
        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public bool IsShowDialogEmpSearch { get; set; }
        public string? StatusIds { get; set; } // Tình trạng nào
        public string EnumEmployeeType { get; set; } = string.Empty; // Hiện có nhân viên lập & nhân viên ký
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn
        public string? VoucherHistory { get; set; } = string.Empty; // lịch sử chứng từ
        public string CssClassHideControl { get; set; } = "d-block"; // ẩn/hiện control 
        // lock control lại
        public bool IsReadonlyControl { get; set; } = false;
        public bool IsShowPrompt { get; set; }
        public string? ReasonDelete { get; set; } // lý do hủy
        // nút quyền
        public bool IsAllowPost { get; set; }
        public bool IsAllowDelete { get; set; }
        public bool IsAllowPut { get; set; }
        public bool IsAllowApproval { get; set; }
        public bool IsShowPromptDeny { get; set; }
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-chung-tu-quyet-dinh");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Nhân sự"),
                        new BreadcrumbModel("Danh sách quyết định", "danh-sach-chung-tu-quyet-dinh"),
                        new BreadcrumbModel("Chi tiết quyết định", isActive: true),
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
                    await ShowLoading(false);
                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        #region Private Functions

        /// <summary>
        /// kiểm tra quyền nút duyệt/từ chối & phải là ông duyệt
        /// </summary>
        /// <returns></returns>
        private async Task checkPermissionApproval()
        {
            string menuId = await GetMenuId("phe-duyet");
            List<string> lstKey = await CheckEventPermission(menuId);
            IsAllowApproval = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_APPROVAL) != null
                && RequestDocument.employeeSignatureId == EmployeeId
                && RequestDocument.statusCode == CommonConstants.STATUS_CODE_APPROVAL_PENDING;
        }

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
        }

        private void initDataAsync(bool isRefresh = false)
        {
            // GÁN DỮ LIỆU MẶC ĐỊNH
            RequestDocument.statusCode = CommonConstants.STATUS_CODE_ADD; // mặc định là chờ xử lý
            RequestDocument.branchId = BranchId;
            RequestDocument.effectiveDate = DateTime.Now;
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
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.token = Token;
                request.branchId = BranchId;
                request.opt = CommonConstants.ENUM_ACTIVE;
                request.process = ProcessConstants.GET_WORKING_BRANCH;
                var getTask1 = _masterDataService.GetDepartmentAsync(UserId, Token, BranchId, opt: CommonConstants.ENUM_ACTIVE); // ds phòng ban
                var getTask2 = _masterDataService.GetPositionAsync(UserId, Token, BranchId, opt: CommonConstants.ENUM_ACTIVE); // ds chức vụ
                var getTask3 = _masterDataService.GetReasonCategoryAsync(UserId, Token, branchId: BranchId, reasonType: GlobalContants.ENUM_REASON_CTQD, opt: CommonConstants.ENUM_ACTIVE); // ds lý do
                var getTask11 = _masterDataService.GetBranchAsync(1, "", opt: CommonConstants.ENUM_PAGE_LOGIN); // danh sách chi nhánh
                var getTask13 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.LoaiQuyetDinh)); // ds loại quyết định
                var getTask5 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiHopDong)); // ds trạng thái
                var getTask12 = _masterDataService.GetMasterAsync<WorkingBranchModel>(request, isShowToast: false); // danh sách chi nhánh
                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask5,
                    getTask11,
                    getTask12,
                    getTask13
                );

                ListCboStatus = await getTask5;
                ListCboDecisionType = await getTask13;
                ListCboBranch = (await getTask11)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
                ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboPosition = (await getTask2)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboWorkingBranch = (await getTask12)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboReason = (await getTask3)?.Select(m => new ComboboxModel() { code = m.id.ToString(), name = m.name })?.ToList();
            }
            catch (Exception) { throw; }
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
                ListCboTitle = (await getTask3)?.Where(m => m.departmentId == departmentId).Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboSubDepartment = (await getTask13)?.Where(m => m.departmentId == departmentId)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "buildComboByDepartmentAsync");
            }
        }

        /// <summary>
        /// kiểm tra dữ liệu trước khi lưu
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="fieldName"></param>
        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            
            if (string.IsNullOrWhiteSpace(RequestDocument.decisionTypeCode))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Loại quyết định");
                fieldName = "decisionTypeCode";
                return;
            }
            if (RequestDocument.effectiveDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Ngày hiệu lực");
                fieldName = "effectiveDate";
                return;
            }
            if (RequestDocument.employeeSignatureId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(RequestDocument.employeeSignatureId);
                return;
            }
            if (string.IsNullOrWhiteSpace(RequestDocument.reasonId))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Lý do");
                fieldName = "txtReasonId";
                return;
            }
            if (RequestDocument.employeeId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Nhân viên");
                fieldName = nameof(RequestDocument.employeeId);
                return;
            }
            if(RequestDocument.decisionTypeCode != QD_NGHI_VIEC)
            {
                // nếu # quyết định nghỉ việc chặn buộc nhập các thông tin mới
                if (RequestDocument.departmentIdNew < 1)
                {
                    errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Phòng ban mới");
                    fieldName = nameof(RequestDocument.departmentIdNew);
                    return;
                }
                if (RequestDocument.positionIdNew < 1)
                {
                    errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chức vụ mới");
                    fieldName = nameof(RequestDocument.positionIdNew);
                    return;
                }
                if (RequestDocument.titleIdNew < 1)
                {
                    errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chức danh mới");
                    fieldName = nameof(RequestDocument.titleIdNew);
                    return;
                }
                if (RequestDocument.subDepartmentIdNew < 1)
                {
                    errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Bộ phận mới");
                    fieldName = nameof(RequestDocument.subDepartmentIdNew);
                    return;
                }
                if (RequestDocument.workingBranchIdNew < 1)
                {
                    errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Nơi làm việc mới");
                    fieldName = nameof(RequestDocument.workingBranchIdNew);
                    return;
                }

            }    
        }

        /// <summary>
        /// Hiểm thị thông tin chi tiết
        /// </summary>
        /// <returns></returns>
        private async Task showVoucher()
        {
            try
            {
                RequestModel request = new RequestModel();
                request.documentId = pDocEntry;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.process = ProcessConstants.GET_DECISION_DOCUMENT;
                var task1 = _workforceService.GetDecisionDocumentAsync(request);
                var task2 = getDocumentHistory();
                await Task.WhenAll(task1, task2);
                List<DecisionDocumentModel>? lstData = await task1;
                if (!lstData.IsNullOrEmpty())
                {
                    RequestDocument = lstData![0];
                    //cho phép chỉnh sữa khi tình trạng là: A (Tạo mới), Y (Đã gửi yêu cầu phê duyệt)
                    IsReadonlyControl = RequestDocument.statusCode != CommonConstants.STATUS_CODE_ADD
                        && RequestDocument.statusCode != CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                    if(RequestDocument.departmentIdNew > 0) await buildComboByDepartmentAsync(RequestDocument.departmentIdNew);
                    // Kiểm tra quyền duyệt
                    IsAllowApproval = false;
                    await checkPermissionApproval();
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
            => VoucherHistory = await _approvalService.GetFunDocumentHistoryAsync(UserId, BranchId, Token, nameof(EnumObjType.DecisionDocuments), pDocEntry);

        /// <summary>
        /// Lưu thông tin chứng từ
        /// </summary>
        /// <param name="isShowToast"></param>
        /// <returns></returns>
        private async Task<int> saveDocument(bool isShowToast = true)
        {
            try
            {
                if(RequestDocument.decisionTypeCode == QD_NGHI_VIEC)
                {
                    RequestDocument.branchIdNew = -1;
                    RequestDocument.departmentIdNew = -1;
                    RequestDocument.positionIdNew = -1;
                    RequestDocument.titleIdNew = -1;
                    RequestDocument.subDepartmentIdNew = -1;
                    RequestDocument.workingBranchIdNew = -1;
                }    
                string processKey = pActionType == nameof(EnumType.Add) ? ProcessConstants.POST_DECISION_DOCUMENT : ProcessConstants.PUT_DECISION_DOCUMENT;
                RequestDocument.branchId = BranchId;
                RequestDocument.userSign = UserId;
                RequestDocument.userSign2 = UserId;
                string json = JsonConvert.SerializeObject(RequestDocument);
                int result = await _workforceService.UpdateLeaveRequestAsync(processKey, UserId, Token, BranchId, json, "", isShowToast: isShowToast);
                return result;
            }
            catch { throw; }
        }

        /// <summary>
        /// Lưu thông tin phê duyệt
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="messageConfirm"></param>
        /// <returns></returns>
        private async Task saveDataApproval(string statusCode)
        {
            try
            {
                await ShowLoading();
                string approvalRemark = "";
                if (statusCode == CommonConstants.STATUS_CODE_DENY
                    || statusCode == CommonConstants.STATUS_CODE_CANCELED)
                {
                    // kiểm tra bắt nhập ghi chú phê duyệt
                    approvalRemark = $"{ReasonDelete}";
                }
                List<ApprovalModel> lstApproval = new List<ApprovalModel>()
                {
                    new ApprovalModel()
                    {
                        id = -1,
                        branchId = RequestDocument.branchId,
                        docEntry = RequestDocument.id,
                        statusCode = statusCode,
                        objType = nameof(EnumObjType.DecisionDocuments),
                        approvalRemark = approvalRemark,
                        remark = approvalRemark,
                        employeeSignatureId = RequestDocument.employeeSignatureId,
                        userSign2 = UserId,
                        employeeId = EmployeeId,
                        userSign = UserId
                    }
                };
                string content = JsonConvert.SerializeObject(lstApproval);
                var result = await _approvalService.UpdateApprovalAsync(ProcessConstants.PUT_APPROVAL, UserId, Token, content, approvalType: statusCode);
                if (result)
                {
                    IsShowPromptDeny = false;
                    await showVoucher();
                }
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "saveDataApproval");
                ShowError(ex.Message);
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion

        #region Protected Functions

        protected async Task OpenPopupHandler(string type = nameof(EmployeeSelected),
            string popupType = nameof(RequestDocument.employeeCode))
        {
            try
            {
                pPopupType = popupType;
                switch (type)
                {
                    case nameof(EmployeeSelected):
                        EnumEmployeeType = popupType == nameof(RequestDocument.employeeSignatureCode) ? CommonConstants.ENUM_EMPLOYEE_SIGNATURE : "";
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
                    case nameof(RequestDocument.employeeCode):
                        RequestDocument.employeeId = employee.id;
                        RequestDocument.employeeCode = employee.code;
                        RequestDocument.employeeName = employee.name;
                        RequestDocument.branchIdCur = employee.branchId;
                        RequestDocument.branchCodeCur = employee.branchCode;
                        RequestDocument.branchNameCur = employee.branchName;
                        RequestDocument.departmentIdCur = employee.departmentId;
                        RequestDocument.departmentCodeCur = employee.departmentCode;
                        RequestDocument.departmentNameCur = employee.departmentName;
                        RequestDocument.positionIdCur = employee.positionId;
                        RequestDocument.positionCodeCur = employee.positionCode;
                        RequestDocument.positionNameCur = employee.positionName;
                        RequestDocument.titleIdCur = employee.titleId ?? -1;
                        RequestDocument.titleCodeCur = employee.titleCode;
                        RequestDocument.titleNameCur = employee.titleName;
                        RequestDocument.subDepartmentIdCur = employee.subDepartmentId ?? -1;
                        RequestDocument.subDepartmentCodeCur = employee.subDepartmentCode;
                        RequestDocument.subDepartmentNameCur = employee.subDepartmentName;
                        RequestDocument.workingBranchIdCur = employee.workingBranchId;
                        RequestDocument.workingBranchCodeCur = employee.workingBranchCode;
                        RequestDocument.workingBranchNameCur = employee.workingBranchName;

                        RequestDocument.branchIdNew = -1;
                        RequestDocument.departmentIdNew = -1;
                        RequestDocument.positionIdNew = -1;
                        RequestDocument.titleIdNew = -1;
                        RequestDocument.subDepartmentIdNew = -1;
                        RequestDocument.workingBranchIdNew = -1;
                        if (RequestDocument.decisionTypeCode != QD_NGHI_VIEC)
                        {
                            if (employee.departmentId > 0)
                            {
                                await ShowLoading();
                                await Task.Delay(75);
                                await buildComboByDepartmentAsync(employee.departmentId);
                            }
                            RequestDocument.branchIdNew = employee.branchId;
                            RequestDocument.departmentIdNew = employee.departmentId;
                            RequestDocument.positionIdNew = employee.positionId;
                            RequestDocument.titleIdNew = employee.titleId ?? -1;
                            RequestDocument.subDepartmentIdNew = employee.subDepartmentId ?? -1;
                            RequestDocument.workingBranchIdNew = employee.workingBranchId;
                        }
                        IsShowDialogEmpSearch = false;
                        break;
                    case nameof(RequestDocument.employeeSignatureCode):
                        RequestDocument.employeeSignatureId = employee.id;
                        RequestDocument.employeeSignatureCode = employee.code;
                        RequestDocument.employeeSignatureName = employee.name;
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
        /// Thay đổi giá trị combobox
        /// </summary>
        /// <param name="value"></param>
        /// <param name="controlID"></param>
        /// <returns></returns>
        protected async Task ComboboxValueChangedHandler(object? value
            , string controlID = nameof(RequestDocument.departmentIdNew))
        {
            try
            {
                switch (controlID)
                {
                    case nameof(RequestDocument.departmentIdNew):
                        await ShowLoading();
                        await Task.Delay(75);
                        int.TryParse($"{value}", out int departmentId);
                        await buildComboByDepartmentAsync(departmentId);
                        RequestDocument.departmentIdNew = departmentId;
                        RequestDocument.titleIdNew = -1;
                        RequestDocument.subDepartmentIdNew = -1;
                        break;
                    case nameof(RequestDocument.decisionTypeCode):
                        if (RequestDocument.departmentIdNew > 0)
                        {
                            await ShowLoading();
                            await Task.Delay(75);
                            await buildComboByDepartmentAsync(RequestDocument.departmentIdNew);
                        }    
                        RequestDocument.decisionTypeCode = value?.ToString();
                        RequestDocument.branchIdNew = -1;
                        RequestDocument.departmentIdNew = -1;
                        RequestDocument.positionIdNew = -1;
                        RequestDocument.titleIdNew = -1;
                        RequestDocument.subDepartmentIdNew = -1;
                        RequestDocument.workingBranchIdNew = -1;
                        if (RequestDocument.decisionTypeCode != QD_NGHI_VIEC 
                            && !string.IsNullOrEmpty(RequestDocument.employeeCode))
                        {
                            RequestDocument.branchIdNew = RequestDocument.branchIdCur;
                            RequestDocument.departmentIdNew = RequestDocument.departmentIdCur;
                            RequestDocument.positionIdNew = RequestDocument.positionIdCur;
                            RequestDocument.titleIdNew = RequestDocument.titleIdCur;
                            RequestDocument.subDepartmentIdNew = RequestDocument.subDepartmentIdCur;
                            RequestDocument.workingBranchIdNew = RequestDocument.workingBranchIdCur;
                        }
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
        /// lưu thông tin chứng từ
        /// </summary>
        /// <returns></returns>
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
                errorMessage += pActionType == nameof(EnumType.Add) ? MessageConstants.MESSAGE_CONFIRM_ADD : MessageConstants.MESSAGE_CONFIRM_UPDATE;
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                int result = await saveDocument();
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
                await checkPermission(MenuId);
                if (!IsAllowDelete)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
                string errorMessage = string.Empty;
                string fieldName = string.Empty; // trả ra trường nào cần validate
                bool isConfirm = true;
                validateForSave(ref errorMessage, ref fieldName);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                    return;
                }
                errorMessage += string.Format(MessageConstants.MESSAGE_CONFIRM_SEND_APPROVAL_FORMAT, $"đến nhân viên {RequestDocument.employeeSignatureName}");
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{errorMessage}");
                if (!isConfirm) return;
                await ShowLoading();
                int result = await saveDocument(isShowToast: false);
                if (result > 0)
                {
                    pActionType = nameof(EnumType.Update);
                    pDocEntry = result;
                    string processKey = ProcessConstants.POST_APPROVAL;
                    ApprovalModel approval = new ApprovalModel();
                    approval.docEntry = pDocEntry;
                    approval.objType = nameof(EnumObjType.DecisionDocuments);
                    approval.branchId = BranchId;
                    approval.statusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                    approval.userSign = UserId;
                    approval.employeeId = EmployeeId;
                    approval.employeeSignatureId = RequestDocument.employeeSignatureId;
                    string content = JsonConvert.SerializeObject(approval);
                    isConfirm = await _approvalService.UpdateApprovalAsync(processKey, UserId, Token, json: content);
                    if (isConfirm)
                    {
                        await showVoucher();
                        return;
                    }
                    await showVoucher();
                }

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
                _navigationManager.NavigateTo($"/chung-tu-de-nghi?key={key}");
                RequestDocument = new DecisionDocumentModel();
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

        /// <summary>
        /// Hủy chứng từ
        /// </summary>
        /// <returns></returns>
        protected async Task CancelDocumentHandler(bool isAccept = false)
        {
            try
            {
                await checkPermission(MenuId);
                if (!IsAllowDelete)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
                if (!isAccept)
                {
                    ReasonDelete = string.Empty;
                    IsShowPrompt = true;
                    return;
                }
                if (string.IsNullOrEmpty(ReasonDelete))
                {
                    ShowWarning(string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Lý do hủy"));
                    return;
                }
                string errorMessage = string.Empty;
                bool isConfirm = true;
                //errorMessage = string.Format(MessageConstants.MESSAGE_CONFIRM_CANCEL_DOCUMENT_FORMAT, $"Phụ lục hợp đồng");
                //isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{errorMessage}");
                //if (!isConfirm) return;
                await ShowLoading();
                string processKey = ProcessConstants.PUT_CANCEL_DOCUMENT;
                ApprovalModel approval = new ApprovalModel();
                approval.docEntry = RequestDocument.id;
                approval.objType = nameof(EnumObjType.DecisionDocuments);
                approval.employeeId = EmployeeId;
                approval.statusCode = CommonConstants.STATUS_CODE_CANCELED;
                approval.approvalRemark = ReasonDelete;
                approval.userSign = UserId;
                var lstApproval = new List<ApprovalModel>() { approval };
                string content = JsonConvert.SerializeObject(lstApproval);
                isConfirm = await _approvalService.UpdateApprovalAsync(processKey, UserId, Token, json: content);
                if (isConfirm)
                {
                    IsShowPrompt = false;
                    await showVoucher();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "CancelDocumentHandler");
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// phê duyệt chứng từ
        /// </summary>
        /// <returns></returns>
        protected async Task ApprovalHandler()
        {
            try
            {
                await checkPermissionApproval();
                if (!IsAllowApproval)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
                bool isConfirm = false;
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, MessageConstants.MESSAGE_CONFIRM_APPROVAL_DOCUMENT);
                if (!isConfirm) return;
                await saveDataApproval(CommonConstants.STATUS_CODE_APPROVED);
            }
            catch { }

        }

        /// <summary>
        /// từ chối chứng từ
        /// </summary>
        /// <returns></returns>
        protected async Task RejectHandler(bool isAccept = false)
        {
            try
            {
                await checkPermissionApproval();
                if (!IsAllowApproval)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
                if (!isAccept)
                {
                    ReasonDelete = string.Empty;
                    IsShowPromptDeny = true;
                    return;
                }
                if (string.IsNullOrEmpty(ReasonDelete))
                {
                    ShowWarning(string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Lý do từ chối"));
                    return;
                }
                await saveDataApproval(CommonConstants.STATUS_CODE_DENY);
            }
            catch { }
        }

        /// <summary>
        /// In quyết định
        /// </summary>
        /// <returns></returns>
        protected async Task PrintDocHandler()
        {
            try
            {
                var decisionType = ListCboDecisionType?.FirstOrDefault(m => m.code == RequestDocument.decisionTypeCode);
                if (decisionType == null) return;
                await ShowLoading();
                var stream = await _masterDataService.PrintDocumentAsync(UserId, Token, BranchId, RequestDocument.id, ProcessConstants.GET_DECISION_DOCUMENT, $"{decisionType.code}.docx");
                if (stream == null) return;
                await _jsRuntime.InvokeAsync<string>("downloadFileFromStream", $"{RequestDocument.voucherNo}-{decisionType.name} với {RequestDocument.employeeName}.docx", GlobalContants.MIME_TYPE_WORD, stream);
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
