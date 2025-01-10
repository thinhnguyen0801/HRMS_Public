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
        public string? DepartmentIds { get; set; }
        public string? StatusIds { get; set; } // Tình trạng nào
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn
        public bool firstRender = true;

        public string? VoucherHistory { get; set; } = string.Empty; // lịch sử chứng từ
        // lock control lại
        public bool IsReadonlyControl { get; set; } = false;

        // nút quyền
        public bool IsAllowPost { get; set; }
        public bool IsAllowDelete { get; set; }
        public bool IsAllowPut { get; set; }
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
                var getTask3 = _masterDataService.GetReasonCategorieAsync(UserId, Token, GlobalContants.ENUM_REASON_DNTC);
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
            => VoucherHistory = await _approvalService.GetFunDocumentHistoryAsync(UserId, BranchId, Token, nameof(EnumObjType.LeaveRequests), pDocEntry);


        /// <summary>
        /// kiểm tra dữ liệu trước khi lưu
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="fieldName"></param>
        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (ListOvertimeDays.IsNullOrEmpty())
            {
                errorMessage = "Không tìm thấy danh sách tăng ca. Vui lòng làm mới danh sách tăng ca!!!";
                fieldName = "gridInfo";
                return;
            }
            // kiểm tra trong lưới dữ liệu hợp lệ chưa
            OvertimeRequest1Model? itemCheck = ListOvertimeDays!.FirstOrDefault(m => m.startTime >= m.endTime);
            if(itemCheck != null)
            {
                errorMessage = $"Ngày [{itemCheck.overtimeDate.ToString(GlobalContants.FORMAT_DATE)}] {MessageConstants.MESSAGE_FROM_TIME_TO_TIME_INVALID}";
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
            // kiểm tra trong lưới dữ liệu hợp lệ chưa
            itemCheck = ListOvertimeDays!.FirstOrDefault(m => m.endBreakTime < m.startBreakTime);
            if (itemCheck != null)
            {
                errorMessage = $"Ngày [{itemCheck.overtimeDate.ToString(GlobalContants.FORMAT_DATE)}] Giờ nghỉ KT không hợp lệ. [Giờ nghỉ BĐ] phải nhỏ hơn [Giờ nghỉ KT]";
                fieldName = "gridInfo";
                return;
            }
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
            if (string.IsNullOrWhiteSpace(OvertimeRequestDocument.reason))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Lý do tăng ca");
                fieldName = nameof(OvertimeRequestDocument.reason);
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
            //if (string.IsNullOrEmpty(OvertimeRequestDocument.shiftCode))
            //{
            //    errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Ca làm việc");
            //    fieldName = "shiftCode";
            //    return;
            //}
        }

        /// <summary>
        /// kiểm tra dữ liệu trươc khi gửi phê duyệt
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="fieldName"></param>
        private void validateForSaveApproval(ref string errorMessage, ref string fieldName)
        {
            if (OvertimeRequestDocument.id < 1)
            {
                errorMessage = "Vui lòng lưu thông tin chứng từ trước khi gửi phê duyệt";
                fieldName = "zzzz";
                return;
            }
            if (OvertimeRequestDocument.employeeSignatureId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(OvertimeRequestDocument.employeeSignatureId);
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
                item.startBreakTime = new DateTime(item.overtimeDate.Year, item.overtimeDate.Month, item.overtimeDate.Day, item.startBreakTime?.Hour ?? 0, item.startBreakTime?.Minute ?? 0, 0);
                item.endBreakTime = new DateTime(item.overtimeDate.Year, item.overtimeDate.Month, item.overtimeDate.Day, item.endBreakTime?.Hour ?? 0, item.endBreakTime?.Minute ?? 0, 0);
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
            request.opt = OvertimeRequestDocument.fromDate!.FormatDateTimeSql();
            request.opt1 = OvertimeRequestDocument.toDate!.FormatDateTimeSql();
            request.opt2 = OvertimeRequestDocument.shiftCode;
            request.type = ProcessConstants.GET_COMBO_LIST_OVERTIME_REQUEST_DAY;
            var result = await _workforceService.GetMasterDataAsync<OvertimeRequest1Model>(request, isShowToast: true);
            ListOvertimeDays = result;
            calcTotalWorkingHour();
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
                        OvertimeRequestDocument.employeeId = employee.id;
                        OvertimeRequestDocument.employeeCode = employee.code;
                        OvertimeRequestDocument.employeeName = employee.name;
                        OvertimeRequestDocument.departmentId = employee.departmentId;
                        IsShowDialogEmpSearch = false;
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
                calcTotalWorkingHour();
                string processKey = pActionType == nameof(EnumType.Add) ? ProcessConstants.POST_OVERTIME_REQUEST : ProcessConstants.PUT_OVERTIME_REQUEST;
                OvertimeRequestDocument.branchId = BranchId;
                OvertimeRequestDocument.userSign = UserId;
                OvertimeRequestDocument.userSign2 = UserId;
                string json = JsonConvert.SerializeObject(OvertimeRequestDocument);
                string jsonDetail = JsonConvert.SerializeObject(ListOvertimeDays);
                int result = await _workforceService.UpdateLeaveRequestAsync(processKey, UserId, Token, BranchId, json, jsonDetail);
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
                validateForSaveApproval(ref errorMessage, ref fieldName);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                    return;
                }
                await Task.Yield();
                errorMessage = string.Format(MessageConstants.MESSAGE_CONFIRM_SEND_APPROVAL_FORMAT, $"đến nhân viên {OvertimeRequestDocument.employeeSignatureName}");
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{errorMessage}");
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = ProcessConstants.POST_APPROVAL;
                ApprovalModel approval = new ApprovalModel();
                approval.docEntry = OvertimeRequestDocument.id;
                approval.objType = nameof(EnumObjType.OvertimeRequests);
                approval.branchId = BranchId;
                approval.statusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                approval.userSign = UserId;
                approval.employeeSignatureId = OvertimeRequestDocument.employeeSignatureId;
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
                // Tính tổng giờ làm việc
                double totalWorkHours = 0;
                if(itemFind.startBreakTime != null && itemFind.endBreakTime != null)
                {
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
                _navigationManager.NavigateTo($"/dang-ky-doi-ca?key={key}");
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
        #endregion

    }
}
