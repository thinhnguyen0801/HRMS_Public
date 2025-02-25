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
    public class LeaveRequestController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IWorkforceService _workforceService { get; init; }
        [Inject] IApprovalService _approvalService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        const string STRING_KEY_EVENT_POST = "LEAVE_REQUEST_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "LEAVE_REQUEST_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "LEAVE_REQUEST_CONTROLLER_DELETE";
        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public int ActiveTabIndex { get; set; } = 0;
        public LeaveRequestModel LeaveRequestDocument { get; set; } = new LeaveRequestModel();
        public List<LeaveRequest1Model>? ListOfVacationDays { get; set; } // danh sách thông tin lương
        public IGrid? GridOfVacationDays { get; set; }
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public List<ComboboxModel>? ListCboReason { get; set; } // cbo ds phòng ban
        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // cbo ds tình trạng

        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public bool IsShowDialogEmpSearch { get; set; }
        public string? DepartmentIds { get; set; }
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
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-de-nghi-nghi-phep");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    this.firstRender = firstRender;
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Công - Phép"),
                        new BreadcrumbModel("Chứng từ đề nghị"),
                        new BreadcrumbModel("Đề nghị nghỉ phép", "danh-sach-de-nghi-nghi-phep"),
                        new BreadcrumbModel("Chi tiết đề nghị nghỉ phép", isActive: true),
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    //
                    initDataAsync();
                    await buildComboAsync();
                    await getAnnualLeaveInfo(EmployeeId);
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
            LeaveRequestDocument.statusCode = CommonConstants.STATUS_CODE_ADD; // mặc định là chờ xử lý
            LeaveRequestDocument.employeeId = EmployeeId;
            LeaveRequestDocument.employeeCode = EmployeeCode;
            LeaveRequestDocument.employeeName = EmployeeName;
            LeaveRequestDocument.departmentId = DepartmentId;
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
                var getTask2 = _masterDataService.GetReasonCategoryAsync(UserId, Token, GlobalContants.ENUM_REASON_DNNP, opt: CommonConstants.ENUM_ACTIVE);
                var getTask5 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiHopDong)); // ds trạng thái
                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask5
                );

                ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboReason = (await getTask2)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboStatus = await getTask5;
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
            if (ListOfVacationDays.IsNullOrEmpty())
            {
                errorMessage = "Không tìm thấy danh sách ngày nghỉ. Vui lòng làm mới danh sách ngày nghỉ!!!";
                fieldName = "gridInfo";
                return;
            }
            if (LeaveRequestDocument.employeeId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Nhân viên");
                fieldName = nameof(LeaveRequestDocument.employeeId);
                return;
            }
            if (LeaveRequestDocument.departmentId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Phòng ban");
                fieldName = nameof(LeaveRequestDocument.departmentId);
                return;
            }
            if (LeaveRequestDocument.employeeSignatureId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(LeaveRequestDocument.employeeSignatureId);
                return;
            }
            if (LeaveRequestDocument.reasonId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Lý do nghỉ");
                fieldName = nameof(LeaveRequestDocument.reasonId);
                return;
            }
            validateForCreateLeaveDate(ref errorMessage, ref fieldName);
        }

        /// <summary>
        /// kiểm tra từ ngày đến ngày
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="fieldName"></param>
        private void validateForCreateLeaveDate(ref string errorMessage, ref string fieldName)
        {
            if (LeaveRequestDocument.fromDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Từ ngày"); ;
                fieldName = "startDate";
                return;
            }
            if (LeaveRequestDocument.toDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Đến ngày"); ;
                fieldName = "endDate";
                return;
            }
            if (LeaveRequestDocument.toDate.Value.Date < LeaveRequestDocument.fromDate.Value.Date)
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
            if (LeaveRequestDocument.id < 1)
            {
                errorMessage = "Vui lòng lưu thông tin chứng từ trước khi gửi phê duyệt";
                fieldName = "zzzz";
                return;
            }
            if (LeaveRequestDocument.employeeSignatureId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(LeaveRequestDocument.employeeSignatureId);
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
                request.process = ProcessConstants.GET_LEAVE_REQUEST;
                var task1 = _workforceService.GetLeaveRequestAsync(request);
                var task2 = getDocumentHistory();
                await Task.WhenAll(task1, task2);
                List<LeaveRequestModel>? lstData = await task1;
                if (!lstData.IsNullOrEmpty())
                {
                    LeaveRequestDocument = lstData![0];
                    await getAnnualLeaveInfo(LeaveRequestDocument.employeeId); // lấy thông tin ngày nghỉ
                    //cho phép chỉnh sữa khi tình trạng là: A (Tạo mới), Y (Đã gửi yêu cầu phê duyệt)
                    IsReadonlyControl = LeaveRequestDocument.statusCode != CommonConstants.STATUS_CODE_ADD
                        && LeaveRequestDocument.statusCode != CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                    if (!string.IsNullOrEmpty(LeaveRequestDocument.jsonDetail))
                    {
                        ListOfVacationDays = JsonConvert.DeserializeObject<List<LeaveRequest1Model>>(LeaveRequestDocument.jsonDetail);
                        //GridOfVacationDays?.Reload();
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
        /// lấy thông tin phép năm
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        private async Task getAnnualLeaveInfo(int employeeId)
        {
            try
            {
                LeaveRequestDocument.numOfLeave = 0;
                LeaveRequestDocument.numOfLeaveLevel = 0;
                LeaveRequestDocument.numOfLeaveOld = 0;
                LeaveRequestDocument.numOfLeaveTotal = 0;
                LeaveRequestDocument.numOfLeaveUsed = 0;
                LeaveRequestDocument.numOfLeaveRemaining = 0;
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_ANNUAL_LEAVE_INFO;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.opt = DateTime.Now.Year.ToString(); // lấy năm hiện tại
                request.departmentIds = "";
                request.opt1 = ""; // phòng ban
                request.opt2 = ""; 
                request.opt3 = employeeId.ToString(); // nhân viên
                var result = await _workforceService.GetMasterDataAsync<AnnualLeaveInfoModel>(request);
                if(!result.IsNullOrEmpty())
                {
                    var header = result![0];
                    LeaveRequestDocument.numOfLeave = header.numOfLeave;
                    LeaveRequestDocument.numOfLeaveLevel = header.numOfLeaveLevel;
                    LeaveRequestDocument.numOfLeaveOld = header.numOfLeaveOld;
                    LeaveRequestDocument.numOfLeaveTotal = header.numOfLeaveTotal;
                    LeaveRequestDocument.numOfLeaveUsed = header.numOfLeaveUsed;
                    LeaveRequestDocument.numOfLeaveRemaining = header.numOfLeaveRemaining;
                }    
            }
            catch(Exception){ throw; }
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
            request.opt = LeaveRequestDocument.fromDate!.FormatDateTimeSql();
            request.opt1 = LeaveRequestDocument.toDate!.FormatDateTimeSql();
            request.type = ProcessConstants.GET_COMBO_LIST_OF_VACATION_DAY;
            var result = await _workforceService.GetMasterDataAsync<LeaveRequest1Model>(request, isShowToast: true);
            ListOfVacationDays = result;
        }
        #endregion


        #region Protected Functions
        protected async Task OpenPopupHandler(string type = nameof(EmployeeSelected), 
            string popupType = nameof(LeaveRequestDocument.employeeCode))
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
                    case nameof(LeaveRequestDocument.employeeCode):
                        await ShowLoading();
                        await getAnnualLeaveInfo(employee.id);
                        LeaveRequestDocument.employeeId = employee.id;
                        LeaveRequestDocument.employeeCode = employee.code;
                        LeaveRequestDocument.employeeName = employee.name;
                        LeaveRequestDocument.departmentId = employee.departmentId;
                        IsShowDialogEmpSearch = false;
                        await Task.Delay(75);
                        break;
                    case nameof(LeaveRequestDocument.employeeSignatureCode):
                        LeaveRequestDocument.employeeSignatureId = employee.id;
                        LeaveRequestDocument.employeeSignatureCode = employee.code;
                        LeaveRequestDocument.employeeSignatureName = employee.name;
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
        /// tạo danh sách ngày nghỉ
        /// </summary>
        /// <returns></returns>
        protected async Task CreateLeaveDateHandler()
        {
            try
            {
                string errorMessage = string.Empty;
                string fieldName = string.Empty; // trả ra trường nào cần validate
                validateForCreateLeaveDate(ref errorMessage, ref fieldName);
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

        protected void GridLeaveDateEditSavingHandler(GridEditModelSavingEventArgs e)
        {
            try
            {
                if (IsReadonlyControl) return;
                var itemEdit = (LeaveRequest1Model)e.EditModel;
                var itemFind = ListOfVacationDays?.FirstOrDefault(m => m.dateOff == itemEdit.dateOff && m.id == itemEdit.id);
                if (itemFind == null) return;
                itemFind.remark = itemEdit.remark;
                itemFind.isMorningBreak = itemEdit.isMorningBreak;
                itemFind.isAfternoonBreak = itemEdit.isAfternoonBreak;
                StateHasChanged();
            }
            catch(Exception ex)
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
                if(e.ElementType == GridElementType.DataRow && GridOfVacationDays != null)
                {
                    var employee = (LeaveRequest1Model)GridOfVacationDays.GetDataItem(e.VisibleIndex);
                    if(!string.IsNullOrEmpty(employee?.bgColor))
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
                string processKey = pActionType == nameof(EnumType.Add) ? ProcessConstants.POST_LEAVE_REQUEST : ProcessConstants.PUT_LEAVE_REQUEST;
                LeaveRequestDocument.branchId = BranchId;
                LeaveRequestDocument.userSign = UserId;
                LeaveRequestDocument.userSign2 = UserId;
                string json = JsonConvert.SerializeObject(LeaveRequestDocument);
                string jsonDetail = JsonConvert.SerializeObject(ListOfVacationDays);
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
                errorMessage = string.Format(MessageConstants.MESSAGE_CONFIRM_SEND_APPROVAL_FORMAT, $"đến nhân viên {LeaveRequestDocument.employeeSignatureName}");
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{errorMessage}");
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = ProcessConstants.POST_APPROVAL;
                ApprovalModel approval = new ApprovalModel();
                approval.docEntry = LeaveRequestDocument.id;
                approval.objType = nameof(EnumObjType.LeaveRequests);
                approval.branchId = BranchId;
                approval.statusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                approval.userSign = UserId;
                approval.employeeId = EmployeeId;
                approval.employeeSignatureId = LeaveRequestDocument.employeeSignatureId;
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
            , string controlID = nameof(LeaveRequestDocument.fromDate))
        {
            try
            {
                if (firstRender) return;
                switch (controlID)
                {
                    case nameof(LeaveRequestDocument.fromDate):
                        LeaveRequestDocument.fromDate = (DateTime?)value;
                        LeaveRequestDocument.toDate = null;
                        ListOfVacationDays = new List<LeaveRequest1Model>();
                        break;
                    case nameof(LeaveRequestDocument.toDate):
                        LeaveRequestDocument.toDate = (DateTime?)value;
                        ListOfVacationDays = new List<LeaveRequest1Model>();
                        if (LeaveRequestDocument.fromDate != null 
                            && LeaveRequestDocument.toDate != null
                            && LeaveRequestDocument.toDate.Value.Date >= LeaveRequestDocument.fromDate.Value.Date)
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
                _navigationManager.NavigateTo($"/de-nghi-nghi-phep?key={key}");
                LeaveRequestDocument = new LeaveRequestModel();
                ListOfVacationDays = new List<LeaveRequest1Model>();
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
                approval.docEntry = LeaveRequestDocument.id;
                approval.objType = nameof(EnumObjType.LeaveRequests);
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
        #endregion
    }
}
