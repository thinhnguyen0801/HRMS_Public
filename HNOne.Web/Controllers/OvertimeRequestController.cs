using HNOne.Web.Components.Controls;
using HNOne.Web.Services.Interfaces;
using HNOne.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HNOne.Web.Commons;
using HNOne.Web.Models;
using HNOne.Common;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using HNOne.Model.Models;
using HNOne.Model;
using DevExpress.Blazor;

namespace HNOne.Web.Controllers
{
    public class OvertimeRequestController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IWorkforceService _workforceService { get; init; }
        [Inject] IApprovalService _approvalService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        const string STRING_KEY_EVENT_POST = "OVERTIME_REQUEST_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "OVERTIME_REQUEST_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "OVERTIME_REQUEST_CONTROLLER_DELETE";
        const string STRING_KEY_EVENT_CANCEL = "OVERTIME_REQUEST_CONTROLLER_CANCEL";
        const string STRING_KEY_EVENT_APPROVAL = "APPROVAL_CONTROLLER_PUT";
        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public int ActiveTabIndex { get; set; } = 0;
        public OvertimeRequestModel OvertimeRequestDocument { get; set; } = new OvertimeRequestModel();
        public List<OvertimeRequest1Model>? ListOvertimeDays { get; set; } // danh sách thông tin tăng ca
        public IGrid? GridOvertimeDays { get; set; }
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public List<ComboboxModel>? ListRequestType { get; set; } // cbo ds loại tăng ca
        public List<EnumCatagoryModel>? ListCboShift { get; set; } // cbo ds ca làm việc

        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public bool IsShowDialogEmpSearch { get; set; }
        public string EnumEmployeeType { get; set; } = string.Empty; // Hiện có nhân viên lập & nhân viên ký
        public string? StatusIds { get; set; } // Tình trạng nào
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn
        public bool firstRender = true;

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
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-de-nghi-lam-them");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    this.firstRender = firstRender;
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Công - Phép"),
                        new BreadcrumbModel("Chứng từ đề nghị"),
                        new BreadcrumbModel("Đề nghị làm thêm", "danh-sach-de-nghi-lam-them"),
                        new BreadcrumbModel("Chi tiết đề nghị làm thêm", isActive: true),
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
                    this.firstRender = false;
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
                && OvertimeRequestDocument.employeeSignatureId == EmployeeId
                && OvertimeRequestDocument.statusCode == CommonConstants.STATUS_CODE_APPROVAL_PENDING;
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
            OvertimeRequestDocument.statusCode = CommonConstants.STATUS_CODE_ADD; // mặc định là chờ xử lý
            OvertimeRequestDocument.employeeId = EmployeeId;
            OvertimeRequestDocument.employeeCode = EmployeeCode;
            OvertimeRequestDocument.employeeName = EmployeeName;
            OvertimeRequestDocument.departmentId = DepartmentId;
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
                var getTask2 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.CaLamViec)); // ds trạng thái
                var getTask3 = _masterDataService.GetReasonCategoryAsync(UserId, Token, branchId: BranchId, reasonType: GlobalContants.ENUM_REASON_DNTC, opt: CommonConstants.ENUM_ACTIVE);
                await Task.WhenAll(
                    getTask1,
                    getTask5,
                    getTask2,
                    getTask3
                );

                ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboStatus = await getTask5;
                ListCboShift = await getTask2;
                ListRequestType = (await getTask3)?.Select(m => new ComboboxModel() { code = m.id.ToString(), name = m.name })?.ToList();
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
                request.process = ProcessConstants.GET_OVERTIME_REQUEST;
                var task1 = _workforceService.GetOvertimeRequestAsync(request);
                var task2 = getDocumentHistory();
                await Task.WhenAll(task1, task2);
                List<OvertimeRequestModel>? lstData = await task1;
                if (!lstData.IsNullOrEmpty())
                {
                    OvertimeRequestDocument = lstData![0];
                    //cho phép chỉnh sữa khi tình trạng là: A (Tạo mới), Y (Đã gửi yêu cầu phê duyệt)
                    IsReadonlyControl = OvertimeRequestDocument.statusCode != CommonConstants.STATUS_CODE_ADD
                        && OvertimeRequestDocument.statusCode != CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                    if (!string.IsNullOrEmpty(OvertimeRequestDocument.jsonDetail))
                    {
                        ListOvertimeDays = JsonConvert.DeserializeObject<List<OvertimeRequest1Model>>(OvertimeRequestDocument.jsonDetail);
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
            => VoucherHistory = await _approvalService.GetFunDocumentHistoryAsync(UserId, BranchId, Token, nameof(EnumObjType.OvertimeRequests), pDocEntry);


        /// <summary>
        /// kiểm tra dữ liệu trước khi lưu
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="fieldName"></param>
        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (OvertimeRequestDocument.employeeId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Nhân viên");
                fieldName = nameof(OvertimeRequestDocument.employeeId);
                return;
            }
            if (OvertimeRequestDocument.departmentId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Phòng ban");
                fieldName = nameof(OvertimeRequestDocument.departmentId);
                return;
            }
            if (OvertimeRequestDocument.employeeSignatureId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(OvertimeRequestDocument.employeeSignatureId);
                return;
            }
            if (string.IsNullOrEmpty(OvertimeRequestDocument.requestType))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Loại đăng ký");
                fieldName = "requestType";
                return;
            }
            validateForCreateOvertimeDate(ref errorMessage, ref fieldName);
            if (!string.IsNullOrEmpty(errorMessage)) return;
            if (string.IsNullOrWhiteSpace(OvertimeRequestDocument.reason))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Lý do tăng ca");
                fieldName = nameof(OvertimeRequestDocument.reason);
                return;
            }
            if (ListOvertimeDays.IsNullOrEmpty())
            {
                errorMessage = "Không tìm thấy danh sách tăng ca. Vui lòng làm mới danh sách tăng ca!!!";
                fieldName = "gridInfo";
                return;
            }
            // kiểm tra dữ liệu bảng công đã có thông tin chưa
            OvertimeRequest1Model? itemCheck = ListOvertimeDays!.FirstOrDefault(m => string.IsNullOrEmpty(m.shiftCode1));
            if (itemCheck != null)
            {
                errorMessage = $"Vui lòng liên hệ quản trị viên phát sinh dữ liệu công làm việc tháng {itemCheck.overtimeDate.Month} năm {itemCheck.overtimeDate.Year}";
                fieldName = "gridInfo";
                return;
            }
            itemCheck = ListOvertimeDays!.FirstOrDefault(m => m.startTime >= m.endTime);
            if (itemCheck != null)
            {
                errorMessage = $"Ngày [{itemCheck.overtimeDate.ToString(GlobalContants.FORMAT_DATE)}] {MessageConstants.MESSAGE_FROM_TIME_TO_TIME_INVALID}";
                fieldName = "gridInfo";
                return;
            }
            // Kiểm tra nhập Từ giờ và đến giờ phải ở ngoài ca làm việc mặc định loại trừ ngày nghỉ
            // Giao nhau xảy ra nếu khoảng tăng ca cắt vào khoảng ca làm việc
            itemCheck = ListOvertimeDays!.FirstOrDefault(m => !m.isDayOff && (m.startTime < m.endDate1 && m.endTime > m.startDate1));
            if (itemCheck != null)
            {
                errorMessage = $"Ngày [{itemCheck.overtimeDate.ToString(GlobalContants.FORMAT_DATE)}] Từ giờ & Đến giờ không được phép nằm trong ca làm việc mặc định";
                fieldName = "gridInfo";
                return;
            }    
            itemCheck = ListOvertimeDays!.FirstOrDefault(m => m.startBreakTime!.Value < m.startTime || m.endBreakTime!.Value > m.endTime);
            if (itemCheck != null)
            {
                errorMessage = $"Ngày [{itemCheck.overtimeDate.ToString(GlobalContants.FORMAT_DATE)}] " +
                    $"Thời gian nghỉ phải nằm trong khoản [{itemCheck.startTime.ToString(GlobalContants.FORMAT_TIME)}] & [{itemCheck.endTime.ToString(GlobalContants.FORMAT_TIME)}]";
                fieldName = "gridInfo";
                return;
            }
            itemCheck = ListOvertimeDays!.FirstOrDefault(m => m.endBreakTime < m.startBreakTime);
            if (itemCheck != null)
            {
                errorMessage = $"Ngày [{itemCheck.overtimeDate.ToString(GlobalContants.FORMAT_DATE)}] Giờ nghỉ KT không hợp lệ. [Giờ nghỉ BĐ] phải nhỏ hơn [Giờ nghỉ KT]";
                fieldName = "gridInfo";
                return;
            }
            itemCheck = ListOvertimeDays!.FirstOrDefault(m => m.totalWorkingHours <= 0);
            if (itemCheck != null)
            {
                errorMessage = $"Ngày [{itemCheck.overtimeDate.ToString(GlobalContants.FORMAT_DATE)}] Số giờ tăng ca không hợp lệ";
                fieldName = "gridInfo";
                return;
            }
        }

        /// <summary>
        /// kiểm tra từ ngày đến ngày
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="fieldName"></param>
        private void validateForCreateOvertimeDate(ref string errorMessage, ref string fieldName)
        {
            if (OvertimeRequestDocument.fromDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Từ ngày");
                fieldName = "startDate";
                return;
            }
            if (OvertimeRequestDocument.toDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Đến ngày");
                fieldName = "endDate";
                return;
            }
            if (OvertimeRequestDocument.toDate.Value.Date < OvertimeRequestDocument.fromDate.Value.Date)
            {
                errorMessage = MessageConstants.MESSAGE_FROM_DATE_TO_DATE_INVALID;
                fieldName = "startDate";
                return;
            }
            if (OvertimeRequestDocument.fromDate.Value.Year != OvertimeRequestDocument.toDate.Value.Year)
            {
                errorMessage = "Không được đăng ký nghỉ trong giờ ở 2 năm khác nhau";
                fieldName = "endDate";
                return;
            }
        }

        /// <summary>
        /// cập nhật lại thời gian cho đúng với ngày đăng kí + tổng số giờ làm việc
        /// </summary>
        private void calcTotalWorkingHour()
        {
            OvertimeRequestDocument.totalHours = 0;
            if (ListOvertimeDays.IsNullOrEmpty()) return;
            double totalHours = 0;
            foreach(var item in ListOvertimeDays!)
            {
                item.startTime = new DateTime(item.overtimeDate.Year, item.overtimeDate.Month, item.overtimeDate.Day, item.startTime.Hour, item.startTime.Minute, 0);
                item.endTime = new DateTime(item.overtimeDate.Year, item.overtimeDate.Month, item.overtimeDate.Day, item.endTime.Hour, item.endTime.Minute, 0);
                if(item.totalBreakTimeMinutes > 0)
                {
                    var startBT = item.startTime;
                    item.startBreakTime = startBT;
                    item.endBreakTime = startBT.AddMinutes(item.totalBreakTimeMinutes);
                }
                //item.startBreakTime = new DateTime(item.overtimeDate.Year, item.overtimeDate.Month, item.overtimeDate.Day, item.startBreakTime?.Hour ?? 0, item.startBreakTime?.Minute ?? 0, 0);
                //item.endBreakTime = new DateTime(item.overtimeDate.Year, item.overtimeDate.Month, item.overtimeDate.Day, item.endBreakTime?.Hour ?? 0, item.endBreakTime?.Minute ?? 0, 0);
                TimeSpan workBeforeBreak = item.startBreakTime!.Value - item.startTime;
                TimeSpan workAfterBreak = item.endTime - item.endBreakTime!.Value;
                totalHours += workBeforeBreak.TotalHours + workAfterBreak.TotalHours;
            }
            OvertimeRequestDocument.totalHours = totalHours;
        }

        /// <summary>
        /// tạo ra danh sách ngày chi tiết
        /// </summary>
        /// <returns></returns>
        private async Task generateListDays()
        {
            RequestModel request = new RequestModel();
            request.process = ProcessConstants.GET_WORKFORCE_MASTER_DATA;
            request.userId = UserId;
            request.token = Token;
            request.branchId = BranchId;
            request.employeeId = EmployeeId;
            request.opt = OvertimeRequestDocument.fromDate!.FormatDateTimeSql();
            request.opt1 = OvertimeRequestDocument.toDate!.FormatDateTimeSql();
            request.opt2 = OvertimeRequestDocument.shiftCode;
            request.type = ProcessConstants.GET_COMBO_LIST_OVERTIME_REQUEST_DAY;
            var result = await _workforceService.GetMasterDataAsync<OvertimeRequest1Model>(request, isShowToast: true);
            ListOvertimeDays = result;
            calcTotalWorkingHour();
        }

        /// <summary>
        /// Lưu thông tin chứng từ
        /// </summary>
        /// <returns></returns>
        private async Task<int> saveDocument(bool isShowToast = true)
        {
            try
            {
                calcTotalWorkingHour();
                string processKey = pActionType == nameof(EnumType.Add) ? ProcessConstants.POST_OVERTIME_REQUEST : ProcessConstants.PUT_OVERTIME_REQUEST;
                OvertimeRequestDocument.branchId = BranchId;
                OvertimeRequestDocument.userSign = UserId;
                OvertimeRequestDocument.userSign2 = UserId;
                string json = JsonConvert.SerializeObject(OvertimeRequestDocument);
                string jsonDetail = JsonConvert.SerializeObject(ListOvertimeDays);
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
                        branchId = OvertimeRequestDocument.branchId,
                        docEntry = OvertimeRequestDocument.id,
                        statusCode = statusCode,
                        objType = nameof(EnumObjType.OvertimeRequests),
                        approvalRemark = approvalRemark,
                        remark = approvalRemark,
                        employeeSignatureId = OvertimeRequestDocument.employeeSignatureId,
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
                OvertimeRequestDocument.employeeSignatureId = -1;
                OvertimeRequestDocument.employeeSignatureCode = "";
                OvertimeRequestDocument.employeeSignatureName = "";
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.token = Token;
                request.branchId = BranchId;
                request.type = ProcessConstants.GET_COMBO_TYPE_PREVIOUS_SIGNER_BY_EMPLOYEE;
                request.opt = employeeId.ToString();
                request.opt1 = nameof(EnumObjType.OvertimeRequests);
                var result = await _masterDataService.GetMasterDataAsync<EmployeeModel>(request);
                if (!result.IsNullOrEmpty())
                {
                    var employee = result![0];
                    OvertimeRequestDocument.employeeSignatureId = employee.id;
                    OvertimeRequestDocument.employeeSignatureCode = employee.code;
                    OvertimeRequestDocument.employeeSignatureName = employee.name;
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region Protected Functions
        protected async Task OpenPopupHandler(string type = nameof(EmployeeSelected),
            string popupType = nameof(OvertimeRequestDocument.employeeCode))
        {
            try
            {
                pPopupType = popupType;
                switch (type)
                {
                    case nameof(EmployeeSelected):
                        //ListCboDepartment ??= new();
                        //DepartmentIds = string.Join(",", ListCboDepartment.Select(m => m.id));
                        EnumEmployeeType = popupType == nameof(OvertimeRequestDocument.employeeSignatureCode) ? CommonConstants.ENUM_EMPLOYEE_SIGNATURE : "";
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
                    case nameof(OvertimeRequestDocument.employeeCode):
                        await ShowLoading();
                        await getEmployeeSignatureHistory(employee.id);
                        OvertimeRequestDocument.employeeId = employee.id;
                        OvertimeRequestDocument.employeeCode = employee.code;
                        OvertimeRequestDocument.employeeName = employee.name;
                        OvertimeRequestDocument.departmentId = employee.departmentId;
                        IsShowDialogEmpSearch = false;
                        await Task.Delay(75);
                        break;
                    case nameof(OvertimeRequestDocument.employeeSignatureCode):
                        OvertimeRequestDocument.employeeSignatureId = employee.id;
                        OvertimeRequestDocument.employeeSignatureCode = employee.code;
                        OvertimeRequestDocument.employeeSignatureName = employee.name;
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
                // Nếu là tạo với & kiểm tra lập phiếu trễ
                if (pActionType == nameof(EnumType.Add))
                {
                    var checkOldDate = ListOvertimeDays!.FirstOrDefault(m => m.overtimeDate < DateTime.Now.Date && m.isDayOff == false);
                    if (checkOldDate != null) errorMessage = MessageConstants.MESSAGE_CONFIRM_ADD_OLD_DAY;
                }
                errorMessage += string.Format(MessageConstants.MESSAGE_CONFIRM_SEND_APPROVAL_FORMAT, $"đến nhân viên {OvertimeRequestDocument.employeeSignatureName}");
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{errorMessage}");
                if (!isConfirm) return;
                await ShowLoading();
                int result = await saveDocument(isShowToast: false);
                if(result > 0)
                {
                    pActionType = nameof(EnumType.Update);
                    pDocEntry = result;
                    string processKey = ProcessConstants.POST_APPROVAL;
                    ApprovalModel approval = new ApprovalModel();
                    approval.docEntry = pDocEntry;
                    approval.objType = nameof(EnumObjType.OvertimeRequests);
                    approval.branchId = BranchId;
                    approval.statusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                    approval.userSign = UserId;
                    approval.employeeId = EmployeeId;
                    approval.employeeSignatureId = OvertimeRequestDocument.employeeSignatureId;
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
        /// thay đổi thông tin DateEdit
        /// </summary>
        /// <param name="value"></param>
        /// <param name="controlID"></param>
        protected async void DateEditValueChangedHandler(object? value
            , string controlID = nameof(OvertimeRequestDocument.fromDate))
        {
            try
            {
                if (firstRender) return;
                switch (controlID)
                {
                    case nameof(OvertimeRequestDocument.fromDate):
                        OvertimeRequestDocument.fromDate = (DateTime?)value;
                        OvertimeRequestDocument.toDate = null;
                        ListOvertimeDays = new List<OvertimeRequest1Model>();
                        break;
                    case nameof(OvertimeRequestDocument.toDate):
                        OvertimeRequestDocument.toDate = (DateTime?)value;
                        ListOvertimeDays = new List<OvertimeRequest1Model>();
                        if (OvertimeRequestDocument.fromDate != null
                            && OvertimeRequestDocument.toDate != null
                            && OvertimeRequestDocument.toDate.Value.Date >= OvertimeRequestDocument.fromDate.Value.Date)
                        {
                            await ShowLoading();
                            await generateListDays();
                            await Task.Delay(100);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "SpinEditValueChangeHandler");
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void GridOvertimeDateEditSavingHandler(GridEditModelSavingEventArgs e)
        {
            try
            {
                var itemEdit = (OvertimeRequest1Model)e.EditModel;
                var itemFind = ListOvertimeDays?.FirstOrDefault(m => m.overtimeDate == itemEdit.overtimeDate && m.id == itemEdit.id);
                if (itemFind == null) return;
                itemFind.remark = itemEdit.remark;
                itemFind.startTime = itemEdit.startTime;
                itemFind.endTime = itemEdit.endTime;
                itemFind.startBreakTime = itemEdit.startBreakTime;
                itemFind.endBreakTime = itemEdit.endBreakTime;
                itemFind.totalBreakTimeMinutes = itemEdit.totalBreakTimeMinutes;
                // Tính tổng giờ làm việc
                double totalWorkHours = 0;
                if (itemEdit.totalBreakTimeMinutes > 0)
                {
                    var startBT = itemEdit.startTime;
                    itemFind.startBreakTime = startBT;
                    itemFind.endBreakTime = startBT.AddMinutes(itemEdit.totalBreakTimeMinutes);
                    TimeSpan workBeforeBreak = itemFind.startBreakTime!.Value - itemFind.startTime;
                    TimeSpan workAfterBreak = itemFind.endTime - itemFind.endBreakTime!.Value;
                    totalWorkHours = workBeforeBreak.TotalHours + workAfterBreak.TotalHours;
                }
                itemFind.totalWorkingHours = totalWorkHours;
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
                if (e.ElementType == GridElementType.DataRow && GridOvertimeDays != null)
                {
                    var employee = (OvertimeRequest1Model)GridOvertimeDays.GetDataItem(e.VisibleIndex);
                    if (!string.IsNullOrEmpty(employee?.bgColor))
                    {
                        e.Style = $"background-color: {employee.bgColor}";
                    }
                }
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// tạo danh sách ngày nghỉ
        /// </summary>
        /// <returns></returns>
        protected async Task CreateOvertimeDateHandler()
        {
            try
            {
                string errorMessage = string.Empty;
                string fieldName = string.Empty; // trả ra trường nào cần validate
                validateForCreateOvertimeDate(ref errorMessage, ref fieldName);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                    return;
                }
                await ShowLoading();
                await generateListDays();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "CreateLeaveDateHandler");
            }
            finally
            {
                await Task.Delay(100);
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
                _navigationManager.NavigateTo($"/de-nghi-lam-them?key={key}");
                OvertimeRequestDocument = new OvertimeRequestModel();
                ListOvertimeDays = new List<OvertimeRequest1Model>();
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
                approval.docEntry = OvertimeRequestDocument.id;
                approval.objType = nameof(EnumObjType.OvertimeRequests);
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
