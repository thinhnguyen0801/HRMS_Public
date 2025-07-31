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
    public class ConfirmWorkingDayController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IWorkforceService _workforceService { get; init; }
        [Inject] IApprovalService _approvalService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        [Inject] DataHelperService _dataHelperService { get; set; }
        public W1Confirm confirm { get; set; }

        const string STRING_KEY_EVENT_POST = "CONFIRM_WORKING_DAY_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "CONFIRM_WORKING_DAY_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "CONFIRM_WORKING_DAY_CONTROLLER_DELETE";
        const string STRING_KEY_EVENT_CANCEL = "CONFIRM_WORKING_DAY_CONTROLLER_CANCEL";
        const string STRING_KEY_EVENT_APPROVAL = "APPROVAL_CONTROLLER_PUT";
        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public int ActiveTabIndex { get; set; } = 0;
        public ConfirmWorkingDayModel ConfirmRequestDocument { get; set; } = new ConfirmWorkingDayModel();
        public List<ConfirmWorkingDay1Model>? ListOfVacationDays { get; set; } // danh sách thông tin lương
        public IGrid? GridOfVacationDays { get; set; }
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public List<EnumCatagoryModel>? ListCboRequestType { get; set; } // cbo ds loại đăng kí

        public bool IsShowDialogWDayMissing { get; set; }
        public List<ShiftAssignmentModel>? ListTimesheetDetail { get; set; }
        public IGrid? GridTimesheetDetail { get; set; }
        public IReadOnlyList<object>? ListTimesheetDetails { get; set; }

        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public string EnumEmployeeType { get; set; } = string.Empty; // Hiện có nhân viên lập & nhân viên ký
        public bool IsShowDialogEmpSearch { get; set; }
        public string? StatusIds { get; set; } // Tình trạng nào
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn

        public string? VoucherHistory { get; set; } = string.Empty; // lịch sử chứng từ
        // lock control lại
        public bool IsReadonlyControl { get; set; } = false;

        public bool IsShowPrompt { get; set; }
        public string? ReasonDelete { get; set; } // lý do hủy
        // nút quyền
        public bool IsAllowPost { get; set; }
        public bool IsAllowDelete { get; set; }
        public bool IsAllowPut { get; set; }
        public bool IsAllowCancel { get; set; } // hủy phiếu để văn thư hay pns hủy
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
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-xac-nhan-gio-cong");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Công - Phép"),
                        new BreadcrumbModel("Chứng từ đề nghị"),
                        new BreadcrumbModel("Xác nhận giờ công", "danh-sach-xac-nhan-gio-cong"),
                        new BreadcrumbModel("Chi tiết xác nhận giờ công", isActive: true),
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    //
                    initDataAsync();
                    await buildComboAsync();
                    if (pDocEntry > 0)
                    {
                        await showVoucher();
                    }
                    else
                    {
                        await getEmployeeSignatureHistory(EmployeeId);
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
                && ConfirmRequestDocument.employeeSignatureId == EmployeeId
                && ConfirmRequestDocument.statusCode == CommonConstants.STATUS_CODE_APPROVAL_PENDING;
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
            IsAllowCancel = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_CANCEL) != null;
        }

        private void initDataAsync(bool isRefresh = false)
        {
            // GÁN DỮ LIỆU MẶC ĐỊNH
            ConfirmRequestDocument.statusCode = CommonConstants.STATUS_CODE_ADD; // mặc định là chờ xử lý
            ConfirmRequestDocument.createDate = DateTime.Now;
            ConfirmRequestDocument.workingDate = DateTime.Now;
            ConfirmRequestDocument.employeeId = EmployeeId;
            ConfirmRequestDocument.employeeCode = EmployeeCode;
            ConfirmRequestDocument.employeeName = EmployeeName;
            ConfirmRequestDocument.departmentId = DepartmentId;
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
                var getTask1 = _masterDataService.GetDepartmentAsync(UserId, Token, BranchId, opt: CommonConstants.ENUM_ACTIVE); // ds phòng ban
                var getTask5 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiHopDong)); // ds trạng thái
                var getTask6 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.LoaiDangKyXinNghiTrongGio)); // ds trạng thái
                await Task.WhenAll(
                    getTask1,
                    getTask5,
                    getTask6
                );

                ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboStatus = await getTask5;
                ListCboRequestType = await getTask6;
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
            
            if (ConfirmRequestDocument.departmentId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Phòng ban");
                fieldName = nameof(ConfirmRequestDocument.departmentId);
                return;
            }
            if (ConfirmRequestDocument.employeeId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Nhân viên");
                fieldName = nameof(ConfirmRequestDocument.employeeId);
                return;
            }
            if (ConfirmRequestDocument.employeeSignatureId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(ConfirmRequestDocument.employeeSignatureId);
                return;
            }
            if (string.IsNullOrEmpty(ConfirmRequestDocument.remark))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Lý do");
                fieldName = "txtRemark";
                return;
            }
            if (ListOfVacationDays.IsNullOrEmpty())
            {
                errorMessage = "Không tìm thấy danh sách ngày đăng ký. Vui lòng chọn dữ liệu ngày xác nhận giờ công!!!";
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
                request.process = ProcessConstants.GET_CONFIRM_WORKING_HOUR_REQUEST;
                var task1 = _workforceService.GetMasterDataAsync<ConfirmWorkingDayModel>(request);
                var task2 = getDocumentHistory();
                await Task.WhenAll(task1, task2);
                List<ConfirmWorkingDayModel>? lstData = await task1;
                if (!lstData.IsNullOrEmpty())
                {
                    ConfirmRequestDocument = lstData![0];
                    //cho phép chỉnh sữa khi tình trạng là: A (Tạo mới), Y (Đã gửi yêu cầu phê duyệt)
                    IsReadonlyControl = ConfirmRequestDocument.statusCode != CommonConstants.STATUS_CODE_ADD
                        && ConfirmRequestDocument.statusCode != CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                    if (!string.IsNullOrEmpty(ConfirmRequestDocument.jsonDetail))
                    {
                        ListOfVacationDays = JsonConvert.DeserializeObject<List<ConfirmWorkingDay1Model>>(ConfirmRequestDocument.jsonDetail);
                        GridOfVacationDays?.Reload();
                    }
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
            => VoucherHistory = await _approvalService.GetFunDocumentHistoryAsync(UserId, BranchId, Token, nameof(EnumObjType.ConfirmWorkingDays), pDocEntry);


        /// <summary>
        /// cập nhật lại thời gian cho đúng với ngày đăng kí + tổng số giờ làm việc
        /// </summary>s
        private void calcTotalHour()
        {
            if (ListOfVacationDays.IsNullOrEmpty()) return;
            foreach (var item in ListOfVacationDays!)
            {
                item.fromTime = new DateTime(item.workingDate.Year, item.workingDate.Month, item.workingDate.Day, item.fromTime?.Hour ?? 0, item.fromTime?.Minute ?? 0, 0);
                item.toTime = new DateTime(item.workingDate.Year, item.workingDate.Month, item.workingDate.Day, item.toTime?.Hour ?? 0, item.toTime?.Minute ?? 0, 0);
                if (item.fromTime < item.startTime) item.fromTime = item.startTime;
                if (item.toTime > item.endTime) item.toTime = item.endTime;
            }
        }

        /// <summary>
        /// Lưu thông tin chứng từ
        /// </summary>
        /// <returns></returns>
        private async Task<int> saveDocument(bool isShowToast = true)
        {
            try
            {
                calcTotalHour();
                string processKey = pActionType == nameof(EnumType.Add) ? ProcessConstants.POST_CONFIRM_WORKING_HOUR_REQUEST : ProcessConstants.PUT_CONFIRM_WORKING_HOUR_REQUEST;
                ConfirmRequestDocument.branchId = BranchId;
                ConfirmRequestDocument.userSign = UserId;
                ConfirmRequestDocument.userSign2 = UserId;
                string json = JsonConvert.SerializeObject(ConfirmRequestDocument);
                string jsonDetail = JsonConvert.SerializeObject(ListOfVacationDays);
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
                        branchId = ConfirmRequestDocument.branchId,
                        docEntry = ConfirmRequestDocument.id,
                        statusCode = statusCode,
                        objType = nameof(EnumObjType.ConfirmWorkingDays),
                        approvalRemark = approvalRemark,
                        remark = approvalRemark,
                        employeeSignatureId = ConfirmRequestDocument.employeeSignatureId,
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

        /// <summary>
        /// lấy nhân viên ký của người đó trước đây để tiến hành ký
        /// </summary>
        /// <returns></returns>
        private async Task getEmployeeSignatureHistory(int employeeId)
        {
            try
            {
                ConfirmRequestDocument.employeeSignatureId = -1;
                ConfirmRequestDocument.employeeSignatureCode = "";
                ConfirmRequestDocument.employeeSignatureName = "";
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.token = Token;
                request.branchId = BranchId;
                request.type = ProcessConstants.GET_COMBO_TYPE_PREVIOUS_SIGNER_BY_EMPLOYEE;
                request.opt = employeeId.ToString();
                request.opt1 = nameof(EnumObjType.ConfirmWorkingDays);
                var result = await _masterDataService.GetMasterDataAsync<EmployeeModel>(request);
                if (!result.IsNullOrEmpty())
                {
                    var employee = result![0];
                    ConfirmRequestDocument.employeeSignatureId = employee.id;
                    ConfirmRequestDocument.employeeSignatureCode = employee.code;
                    ConfirmRequestDocument.employeeSignatureName = employee.name;
                }
            }
            catch (Exception) { }
        }
        #endregion


        #region Protected Functions
        protected async Task OpenPopupHandler(string type = nameof(EmployeeSelected),
            string popupType = nameof(ConfirmRequestDocument.employeeCode))
        {
            try
            {
                pPopupType = popupType;
                switch (type)
                {
                    case nameof(EmployeeSelected):
                        //ListCboDepartment ??= new();
                        //DepartmentIds = string.Join(",", ListCboDepartment.Select(m => m.id));
                        EnumEmployeeType = popupType == nameof(ConfirmRequestDocument.employeeSignatureCode) ? CommonConstants.ENUM_EMPLOYEE_SIGNATURE : "";
                        IsShowDialogEmpSearch = true;
                        break;
                    case nameof(ListTimesheetDetail):
                        await ShowLoading();
                        RequestModel request = new RequestModel();
                        request.process = ProcessConstants.GET_WORKING_DAY_MISSING_HOURS;
                        request.userId = UserId;
                        request.branchId = BranchId;
                        request.token = Token;
                        request.opt = ConfirmRequestDocument.workingDate.Year.ToString();
                        request.opt1 = ConfirmRequestDocument.workingDate.Month.ToString();
                        request.employeeId = ConfirmRequestDocument.employeeId;
                        ListTimesheetDetails = null;
                        var response = await _workforceService.GetMasterDataAsync<ShiftAssignmentModel>(request, isShowToast: true);
                        if (!response.IsNullOrEmpty())
                        {
                            foreach (var item in response!)
                            {
                                if (item.docEntry > 0)
                                {
                                    Dictionary<string, string> pParams = new Dictionary<string, string>
                                    {
                                        { "pActionType", nameof(EnumType.Update) },
                                        { "pDocEntry", $"{item.docEntry}" }
                                    };
                                    item.link = _dataHelperService.ListUris[$"{item.objType}"] + _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
                                }
                            }
                            ListTimesheetDetail = response;
                            IsShowDialogWDayMissing = true;
                        }
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
                    case nameof(ConfirmRequestDocument.employeeCode):
                        await ShowLoading();
                        await getEmployeeSignatureHistory(employee.id);
                        ConfirmRequestDocument.employeeId = employee.id;
                        ConfirmRequestDocument.employeeCode = employee.code;
                        ConfirmRequestDocument.employeeName = employee.name;
                        ConfirmRequestDocument.departmentId = employee.departmentId;
                        IsShowDialogEmpSearch = false;
                        await Task.Delay(75);
                        break;
                    case nameof(ConfirmRequestDocument.employeeSignatureCode):
                        ConfirmRequestDocument.employeeSignatureId = employee.id;
                        ConfirmRequestDocument.employeeSignatureCode = employee.code;
                        ConfirmRequestDocument.employeeSignatureName = employee.name;
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

        protected void GridLeaveDateEditSavingHandler(GridEditModelSavingEventArgs e)
        {
            try
            {
                var itemEdit = (ConfirmWorkingDay1Model)e.EditModel;
                var itemFind = ListOfVacationDays?.FirstOrDefault(m => m.workingDate == itemEdit.workingDate && m.id == itemEdit.id);
                if (itemFind == null) return;
                itemFind.remark = itemEdit.remark;
                itemFind.fromTime = itemEdit.fromTime;
                itemFind.toTime = itemEdit.toTime;
                calcTotalHour();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GridLeaveDateEditSavingHandler");
            }
        }

        /// <summary>
        /// custom tô line lưới
        /// </summary>
        /// <param name="e"></param>
        protected void GridLeaveDateCustomizeElement(GridCustomizeElementEventArgs e)
        {
            try
            {
                if (e.ElementType == GridElementType.DataRow && GridOfVacationDays != null)
                {
                    var employee = (LeaveRequest1Model)GridOfVacationDays.GetDataItem(e.VisibleIndex);
                    if (!string.IsNullOrEmpty(employee?.bgColor))
                    {
                        e.Style = $"background-color: {employee.bgColor}";
                    }
                }
            }
            catch (Exception ex) { }
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
                errorMessage = pActionType == nameof(EnumType.Add) ? MessageConstants.MESSAGE_CONFIRM_ADD : MessageConstants.MESSAGE_CONFIRM_UPDATE;
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
                await Task.Yield();
                errorMessage = string.Format(MessageConstants.MESSAGE_CONFIRM_SEND_APPROVAL_FORMAT, $"đến nhân viên {ConfirmRequestDocument.employeeSignatureName}");
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
                    approval.objType = nameof(EnumObjType.ConfirmWorkingDays);
                    approval.branchId = BranchId;
                    approval.statusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                    approval.userSign = UserId;
                    approval.employeeId = EmployeeId;
                    approval.employeeSignatureId = ConfirmRequestDocument.employeeSignatureId;
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
                _navigationManager.NavigateTo($"/xac-nhan-gio-cong?key={key}");
                ConfirmRequestDocument = new ConfirmWorkingDayModel();
                ListOfVacationDays = new List<ConfirmWorkingDay1Model>();
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
        /// lấy ngày công thiếu
        /// </summary>
        /// <returns></returns>
        protected async Task SelectWorkingDayMissing()
        {
            try
            {
                if (ListTimesheetDetails.IsNullOrEmpty()) return;
                ListOfVacationDays ??= new List<ConfirmWorkingDay1Model>();
                foreach (var item in ListTimesheetDetails!.Cast<ShiftAssignmentModel>())
                {
                    if (ListOfVacationDays.Any(m => m.workingDate.Date == item.workingDate!.Value.Date)) continue;
                    ConfirmWorkingDay1Model confirmDay = new ConfirmWorkingDay1Model();
                    confirmDay.workingDate = item.workingDate!.Value;
                    confirmDay.fromTime = item.startDateActual ?? item.startDate;
                    confirmDay.toTime = item.endDateActual ?? item.endDate;
                    confirmDay.shiftCode = item.shiftCode;
                    confirmDay.startTime = item.startDate;
                    confirmDay.endTime = item.endDate;
                    confirmDay.startBreakTime = item.startBreakTime;
                    confirmDay.endBreakTime = item.endBreakTime;
                    confirmDay.totalWorkingHours = item.totalWorkingHours;
                    confirmDay.startTimeActual = item.startDateActual;
                    confirmDay.endTimeActual = item.endDateActual;
                    confirmDay.startBreakTimeActual = item.startBreakTimeActual;
                    confirmDay.endBreakTimeActual = item.endBreakTimeActual;
                    confirmDay.totalWorkingHoursActual = item.totalWorkingHoursActual;
                    confirmDay.totalMissingHours = item.sGT;
                    ListOfVacationDays.Add(confirmDay);
                }
                GridOfVacationDays?.Reload();
                IsShowDialogWDayMissing = false;
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

        protected void DeleteDataHandler()
        {
            try
            {
                if (ListOfVacationDays.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                var lstSelected = GridOfVacationDays!.SelectedDataItems;
                if (lstSelected.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                foreach (ConfirmWorkingDay1Model item in lstSelected) ListOfVacationDays!.Remove(item);
                GridOfVacationDays?.Reload();
                InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "DeleteDataHandler");
                ShowError(ex.Message);
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
                approval.docEntry = ConfirmRequestDocument.id;
                approval.objType = nameof(EnumObjType.ConfirmWorkingDays);
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
