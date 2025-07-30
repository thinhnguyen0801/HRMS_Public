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
    public class AdjustedALRequestController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IWorkforceService _workforceService { get; init; }
        [Inject] IApprovalService _approvalService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        const string STRING_KEY_EVENT_POST = "ADJUSTED_AL_REQUEST_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "ADJUSTED_AL_REQUEST_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "ADJUSTED_AL_REQUEST_CONTROLLER_DELETE";
        const string STRING_KEY_EVENT_APPROVAL = "APPROVAL_CONTROLLER_PUT";

        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public int ActiveTabIndex { get; set; } = 0;
        public AdjustedAnnualLeaveRequestModel RequestDocument { get; set; } = new AdjustedAnnualLeaveRequestModel();
        public List<AdjustedAnnualLeaveRequest1Model>? ListEmployeeAdjusted { get; set; } // danh sách nhân viên điều chỉnh
        public IGrid? GridEmployeeAdjusted { get; set; }
        public List<ComboboxModel>? ListCboYear { get; set; }
        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public string EnumEmployeeType { get; set; } = string.Empty; // Hiện có nhân viên lập & nhân viên ký
        public bool IsShowDialogEmpSearch { get; set; }
        public string? StatusIds { get; set; } // Tình trạng nào
        public GridSelectionMode DxGridEmployeeSelectionMode { get; set; } = GridSelectionMode.Single; // chọn môt/nhiều
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn
        public IReadOnlyList<object>? ListEmpSelected { get; set; } // danh sách nhân viên được chọn
        public string? VoucherHistory { get; set; } = string.Empty; // lịch sử chứng từ
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
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-dieu-chinh-phep-nam");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Công - Phép"),
                        new BreadcrumbModel("Tính công"),
                        new BreadcrumbModel("Điều chỉnh phép", "danh-sach-dieu-chinh-phep-nam"),
                        new BreadcrumbModel("Chi tiết điều chỉnh phép", isActive: true),
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
            RequestDocument.year = DateTime.Now.Year;
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

        /// <summary>
        /// lấy dữ liệu cho combobox
        /// </summary>
        /// <returns></returns>
        private async Task buildComboAsync()
        {
            try
            {
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_WORKFORCE_MASTER_DATA;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.type = ProcessConstants.GET_COMBO_ANNUAL_LEAVE_YEAR;
                var getTask1 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiHopDong)); // ds trạng thái
                var getTask3 = _workforceService.GetMasterDataAsync<ComboboxModel>(request, isShowToast: false);
                await Task.WhenAll(
                    getTask1,
                    getTask3
                );
                ListCboYear = await getTask3;
                ListCboStatus = await getTask1;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// kiểm tra dữ liệu trước khi lưu
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="fieldName"></param>
        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (RequestDocument.employeeSignatureId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(RequestDocument.employeeSignatureId);
                return;
            }
            if (string.IsNullOrWhiteSpace(RequestDocument.remark))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Lý do");
                fieldName = "txtRemark";
                return;
            }
            if (ListEmployeeAdjusted.IsNullOrEmpty())
            {
                errorMessage = "Không tìm thấy danh sách nhân viên điều chỉnh phép!!!";
                fieldName = "gridInfo";
                return;
            }
            // Kiễm tra dữ liệu lưới
            AdjustedAnnualLeaveRequest1Model? itemCheck = ListEmployeeAdjusted!.FirstOrDefault(m => m.numOfAdjustedLeave == 0);
            if(itemCheck != null)
            {
                errorMessage = $"Nhân viên [{itemCheck.employeeCode}]. Vui lòng điền số phép cần điều chỉnh +/-";
                fieldName = "gridInfo";
                return;
            }
            itemCheck = ListEmployeeAdjusted!.FirstOrDefault(m => m.startDate.Year != RequestDocument.year);
            if (itemCheck != null)
            {
                errorMessage = $"Nhân viên [{itemCheck.employeeCode}]. Ngày áp dụng phải nằm trong năm {RequestDocument.year}";
                fieldName = "gridInfo";
                return;
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
                request.process = ProcessConstants.GET_ADJUSTED_ANNUAL_LEAVE_REQUEST;
                var task1 = _workforceService.GetAdjustedALRequestAsync(request);
                var task2 = getDocumentHistory();
                await Task.WhenAll(task1, task2);
                List<AdjustedAnnualLeaveRequestModel>? lstData = await task1;
                if (!lstData.IsNullOrEmpty())
                {
                    RequestDocument = lstData![0];
                    //cho phép chỉnh sữa khi tình trạng là: A (Tạo mới), Y (Đã gửi yêu cầu phê duyệt)
                    IsReadonlyControl = RequestDocument.statusCode != CommonConstants.STATUS_CODE_ADD
                        && RequestDocument.statusCode != CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                    if (!string.IsNullOrEmpty(RequestDocument.jsonDetail))
                    {
                        ListEmployeeAdjusted = JsonConvert.DeserializeObject<List<AdjustedAnnualLeaveRequest1Model>>(RequestDocument.jsonDetail);
                        // Kiểm tra quyền duyệt
                        IsAllowApproval = false;
                        await checkPermissionApproval();
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
            => VoucherHistory = await _approvalService.GetFunDocumentHistoryAsync(UserId, BranchId, Token, nameof(EnumObjType.AdjustedAnnualLeaveRequests), pDocEntry);

        /// <summary>
        /// Lưu thông tin chứng từ
        /// </summary>
        /// <param name="isShowToast"></param>
        /// <returns></returns>
        private async Task<int> saveDocument(bool isShowToast = true)
        {
            try
            {
                string processKey = pActionType == nameof(EnumType.Add) ? ProcessConstants.POST_ADJUSTED_ANNUAL_LEAVE_REQUEST : ProcessConstants.PUT_ADJUSTED_ANNUAL_LEAVE_REQUEST;
                RequestDocument.branchId = BranchId;
                RequestDocument.userSign = UserId;
                RequestDocument.userSign2 = UserId;
                string json = JsonConvert.SerializeObject(RequestDocument);
                string jsonDetail = JsonConvert.SerializeObject(ListEmployeeAdjusted);
                int result = await _workforceService.UpdateLeaveRequestAsync(processKey, UserId, Token, BranchId, json, jsonDetail, isShowToast: isShowToast);
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
                        objType = nameof(EnumObjType.AdjustedAnnualLeaveRequests),
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
            string popupType = nameof(RequestDocument.employeeSignatureCode))
        {
            try
            {
                pPopupType = popupType;
                switch (type)
                {
                    case nameof(EmployeeSelected):
                        EnumEmployeeType = popupType == nameof(RequestDocument.employeeSignatureCode) ? CommonConstants.ENUM_EMPLOYEE_SIGNATURE : "";
                        DxGridEmployeeSelectionMode = GridSelectionMode.Single;
                        IsShowDialogEmpSearch = true;
                        break;
                    case nameof(ListEmpSelected):
                        EnumEmployeeType = "";
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
                    case nameof(RequestDocument.employeeSignatureCode):
                        EmployeeModel employee = (EmployeeModel)EmployeeSelected;
                        RequestDocument.employeeSignatureId = employee.id;
                        RequestDocument.employeeSignatureCode = employee.code;
                        RequestDocument.employeeSignatureName = employee.name;
                        IsShowDialogEmpSearch = false;
                        break;
                    case nameof(ListEmpSelected):
                        if (ListEmpSelected.IsNullOrEmpty()) break;
                        ListEmployeeAdjusted ??= new List<AdjustedAnnualLeaveRequest1Model>();
                        foreach (var item in ListEmpSelected!.Cast<EmployeeModel>())
                        {
                            if (ListEmployeeAdjusted.Any(m => m.employeeCode == item.code)) continue;
                            var otraining1 = new AdjustedAnnualLeaveRequest1Model();
                            otraining1.employeeId = item.id;
                            otraining1.employeeCode = item.code;
                            otraining1.employeeName = item.name;
                            otraining1.startDate = DateTime.Now.Date;
                            otraining1.numOfAdjustedLeave = 0;
                            ListEmployeeAdjusted.Add(otraining1);
                        }
                        GridEmployeeAdjusted?.Reload();
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

        protected void GridAdjustedEditSavingHandler(GridEditModelSavingEventArgs e)
        {
            try
            {
                if (IsReadonlyControl) return;
                var itemEdit = (AdjustedAnnualLeaveRequest1Model)e.EditModel;
                var itemFind = ListEmployeeAdjusted?.FirstOrDefault(m => m.adjustedALId == itemEdit.adjustedALId && m.employeeId == itemEdit.employeeId);
                if (itemFind == null) return;
                itemFind.remark = itemEdit.remark;
                itemFind.startDate = itemEdit.startDate;
                itemFind.numOfAdjustedLeave = itemEdit.numOfAdjustedLeave;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GridAdjustedEditSavingHandler");
            }
        }

        protected void DeleteDataHandler()
        {
            try
            {
                if (ListEmployeeAdjusted.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                var lstSelected = GridEmployeeAdjusted!.SelectedDataItems;
                if (lstSelected.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                foreach (AdjustedAnnualLeaveRequest1Model item in lstSelected) ListEmployeeAdjusted!.Remove(item);
                GridEmployeeAdjusted?.Reload();
                InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "DeleteDataHandler");
                ShowError(ex.Message);
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
                    approval.objType = nameof(EnumObjType.AdjustedAnnualLeaveRequests);
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
                _navigationManager.NavigateTo($"/dieu-chinh-phep-nam?key={key}");
                RequestDocument = new AdjustedAnnualLeaveRequestModel();
                ListEmployeeAdjusted = new List<AdjustedAnnualLeaveRequest1Model>();
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
                await ShowLoading();
                string processKey = ProcessConstants.PUT_CANCEL_DOCUMENT;
                ApprovalModel approval = new ApprovalModel();
                approval.docEntry = RequestDocument.id;
                approval.objType = nameof(EnumObjType.AdjustedAnnualLeaveRequests);
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
        #endregion

    }
}
