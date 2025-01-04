using HNOne.Web.Components.Controls;
using HNOne.Web.Services.Interfaces;
using HNOne.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HNOne.Web.Commons;
using HNOne.Model.Models;
using DevExpress.Blazor;
using HNOne.Model;
using HNOne.Web.Models;
using HNOne.Common;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class ShiftChangeRequestController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IWorkforceService _workforceService { get; init; }
        [Inject] IApprovalService _approvalService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        const string STRING_KEY_EVENT_POST = "SHIFT_CHANGE_REQUEST_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "SHIFT_CHANGE_REQUEST_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "SHIFT_CHANGE_REQUEST_CONTROLLER_DELETE"; // gửi duyệt/hủy phiếu
        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public int ActiveTabIndex { get; set; } = 0;

        public ShiftChangeModel ShiftRequestDocument { get; set; } = new ShiftChangeModel();
        public List<ShiftChange1Model>? ListShiftChange { get; set; }
        public IGrid? GridShiftChange { get; set; }
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // cbo ds tình trạng
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
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-dang-ky-doi-ca");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    this.firstRender = firstRender;
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Công - Phép"),
                        new BreadcrumbModel("Chứng từ đề nghị"),
                        new BreadcrumbModel("Đăng ký đổi ca", "danh-sach-dang-ky-doi-ca"),
                        new BreadcrumbModel("Chi tiết đăng ký đổi ca", isActive: true),
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

        #region Private 
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
            ShiftRequestDocument.statusCode = CommonConstants.STATUS_CODE_ADD; // mặc định là chờ xử lý
            ShiftRequestDocument.employeeId = EmployeeId;
            ShiftRequestDocument.employeeCode = EmployeeCode;
            ShiftRequestDocument.employeeName = EmployeeName;
            ShiftRequestDocument.departmentId = DepartmentId;
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
                var getTask1 = _masterDataService.GetDepartmentAsync(UserId, Token); // ds phòng ban
                var getTask5 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiHopDong)); // ds trạng thái
                var getTask2 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.CaLamViec)); // ds trạng thái
                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask5
                );

                ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboStatus = await getTask5;
                ListCboShift = await getTask2;
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
            if (ListShiftChange.IsNullOrEmpty())
            {
                errorMessage = "Không tìm thấy danh sách đổi ca. Vui lòng làm mới danh sách đổi ca!!!";
                fieldName = "gridInfo";
                return;
            }
            // kiểm tra trong lưới dữ liệu hợp lệ chưa
            ShiftChange1Model? itemCheck = ListShiftChange!.FirstOrDefault(m => string.IsNullOrEmpty(m.shiftCode2) && !m.isDayOff);
            if(itemCheck != null)
            {
                errorMessage = $"Ngày [{itemCheck.dateChange.ToString(GlobalContants.FORMAT_DATE)}] Vui lòng điền thông tin ca thay đổi!!!";
                fieldName = "gridInfo";
                return;
            }    
            // kiểm tra trong lưới dữ liệu hợp lệ chưa
            itemCheck = ListShiftChange!.FirstOrDefault(m => m.shiftCode1 == m.shiftCode2 && !m.isDayOff);
            if (itemCheck != null)
            {
                errorMessage = $"Ngày [{itemCheck.dateChange.ToString(GlobalContants.FORMAT_DATE)}] Ca thay đổi không được phép trùng với ca mặc định!!!";
                fieldName = "gridInfo";
                return;
            }
            if (ShiftRequestDocument.employeeId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Nhân viên");
                fieldName = nameof(ShiftRequestDocument.employeeId);
                return;
            }
            if (ShiftRequestDocument.departmentId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Phòng ban");
                fieldName = nameof(ShiftRequestDocument.departmentId);
                return;
            }
            if (ShiftRequestDocument.employeeSignatureId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(ShiftRequestDocument.employeeSignatureId);
                return;
            }
            if (ShiftRequestDocument.fromDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Từ ngày");
                fieldName = "startDate";
                return;
            }
            if (ShiftRequestDocument.toDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Đến ngày");
                fieldName = "endDate";
                return;
            }
            if (ShiftRequestDocument.toDate.Value.Date < ShiftRequestDocument.fromDate.Value.Date)
            {
                errorMessage = MessageConstants.MESSAGE_FROM_DATE_TO_DATE_INVALID;
                fieldName = "startDate";
                return;
            }
            if (string.IsNullOrWhiteSpace(ShiftRequestDocument.reason))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Lý do đổi ca");
                fieldName = "txtghiChu";
                return;
            }
        }

        /// <summary>
        /// kiểm tra từ ngày đến ngày
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="fieldName"></param>
        private void validateForCreateLeaveDate(ref string errorMessage, ref string fieldName)
        {
            if (ShiftRequestDocument.fromDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Từ ngày");
                fieldName = "startDate";
                return;
            }
            if (ShiftRequestDocument.toDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Đến ngày");
                fieldName = "endDate";
                return;
            }
            if (ShiftRequestDocument.toDate.Value.Date < ShiftRequestDocument.fromDate.Value.Date)
            {
                errorMessage = MessageConstants.MESSAGE_FROM_DATE_TO_DATE_INVALID;
                fieldName = "startDate";
                return;
            }
            //if(string.IsNullOrEmpty(ShiftRequestDocument.shiftCode2))
            //{
            //    errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Ca thay đổi");
            //    fieldName = "shiftCode2";
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
            if (ShiftRequestDocument.id < 1)
            {
                errorMessage = "Vui lòng lưu thông tin chứng từ trước khi gửi phê duyệt";
                fieldName = "zzzz";
                return;
            }
            if (ShiftRequestDocument.employeeSignatureId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(ShiftRequestDocument.employeeSignatureId);
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
                request.process = ProcessConstants.GET_SHIFT_CHANGE_REQUEST;
                var task1 = _workforceService.GetShiftChangeRequestAsync(request);
                var task2 = getDocumentHistory();
                await Task.WhenAll(task1, task2);
                List<ShiftChangeModel>? lstData = await task1;
                if (!lstData.IsNullOrEmpty())
                {
                    ShiftRequestDocument = lstData![0];
                    //cho phép chỉnh sữa khi tình trạng là: A (Tạo mới), Y (Đã gửi yêu cầu phê duyệt)
                    IsReadonlyControl = ShiftRequestDocument.statusCode != CommonConstants.STATUS_CODE_ADD
                        && ShiftRequestDocument.statusCode != CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                    if (!string.IsNullOrEmpty(ShiftRequestDocument.jsonDetail))
                    {
                        ListShiftChange = JsonConvert.DeserializeObject<List<ShiftChange1Model>>(ShiftRequestDocument.jsonDetail);
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
            => VoucherHistory = await _approvalService.GetFunDocumentHistoryAsync(UserId, BranchId, Token, nameof(EnumObjType.ShiftChanges), pDocEntry);

        /// <summary>
        /// tạo ra danh sách ngày chi tiết
        /// </summary>
        /// <returns></returns>
        private async Task generateListDays()
        {
            RequestModel request = new RequestModel();
            request.process = ProcessConstants.GET_WORKFORCE_MASTER_DATA;
            request.employeeId = ShiftRequestDocument.employeeId;
            request.userId = UserId;
            request.token = Token;
            request.opt = ShiftRequestDocument.fromDate!.FormatDateTimeSql();
            request.opt1 = ShiftRequestDocument.toDate!.FormatDateTimeSql();
            request.opt2 = ShiftRequestDocument.shiftCode2;
            request.type = ProcessConstants.GET_COMBO_LIST_OF_SHIFT_CHANGE_DAY;
            var result = await _workforceService.GetMasterDataAsync<ShiftChange1Model>(request, isShowToast: true);
            ListShiftChange = result;
        }
        #endregion

        #region Protected Functions

        protected async Task OpenPopupHandler(string type = nameof(EmployeeSelected),
            string popupType = nameof(ShiftRequestDocument.employeeCode))
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
                    case nameof(ShiftRequestDocument.employeeCode):
                        ShiftRequestDocument.employeeId = employee.id;
                        ShiftRequestDocument.employeeCode = employee.code;
                        ShiftRequestDocument.employeeName = employee.name;
                        ShiftRequestDocument.departmentId = employee.departmentId;
                        IsShowDialogEmpSearch = false;
                        break;
                    case nameof(ShiftRequestDocument.employeeSignatureCode):
                        ShiftRequestDocument.employeeSignatureId = employee.id;
                        ShiftRequestDocument.employeeSignatureCode = employee.code;
                        ShiftRequestDocument.employeeSignatureName = employee.name;
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
                string processKey = pActionType == nameof(EnumType.Add) ? ProcessConstants.POST_SHIFT_CHANGE_REQUEST : ProcessConstants.PUT_SHIFT_CHANGE_REQUEST;
                ShiftRequestDocument.branchId = BranchId;
                ShiftRequestDocument.userSign = UserId;
                ShiftRequestDocument.userSign2 = UserId;
                string json = JsonConvert.SerializeObject(ShiftRequestDocument);
                string jsonDetail = JsonConvert.SerializeObject(ListShiftChange);
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
                errorMessage = string.Format(MessageConstants.MESSAGE_CONFIRM_SEND_APPROVAL_FORMAT, $"đến nhân viên {ShiftRequestDocument.employeeSignatureName}");
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{errorMessage}");
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = ProcessConstants.POST_APPROVAL;
                ApprovalModel approval = new ApprovalModel();
                approval.docEntry = ShiftRequestDocument.id;
                approval.objType = nameof(EnumObjType.ShiftChanges);
                approval.branchId = BranchId;
                approval.statusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                approval.userSign = UserId;
                approval.employeeSignatureId = ShiftRequestDocument.employeeSignatureId;
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

        protected async Task CreateShiftChangeDateHandler()
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
                var itemEdit = (ShiftChange1Model)e.EditModel;
                var itemFind = ListShiftChange?.FirstOrDefault(m => m.dateChange == itemEdit.dateChange && m.id == itemEdit.id);
                if (itemFind == null) return;
                itemFind.remark = itemEdit.remark;
                itemFind.shiftCode2 = itemEdit.shiftCode2;
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
                if (e.ElementType == GridElementType.DataRow && GridShiftChange != null)
                {
                    var employee = (ShiftChange1Model)GridShiftChange.GetDataItem(e.VisibleIndex);
                    if (!string.IsNullOrEmpty(employee?.bgColor))
                    {
                        e.Style = $"background-color: {employee.bgColor}";
                    }
                }
            }
            catch (Exception ex) { }
        }

        protected async void DateEditValueChangedHandler(object? value
            , string controlID = nameof(ShiftRequestDocument.fromDate))
        {
            try
            {
                if (firstRender) return;
                switch (controlID)
                {
                    case nameof(ShiftRequestDocument.fromDate):
                        ShiftRequestDocument.fromDate = (DateTime?)value;
                        ShiftRequestDocument.toDate = null;
                        ListShiftChange = new List<ShiftChange1Model>();
                        StateHasChanged();
                        break;
                    case nameof(ShiftRequestDocument.toDate):
                        ShiftRequestDocument.toDate = (DateTime?)value;
                        ListShiftChange = new List<ShiftChange1Model>();
                        if (ShiftRequestDocument.fromDate != null
                            && ShiftRequestDocument.toDate != null
                            && ShiftRequestDocument.toDate.Value.Date >= ShiftRequestDocument.fromDate.Value.Date)
                        {
                            await ShowLoading();
                            await generateListDays();
                            await Task.Delay(100);
                        }
                        StateHasChanged();
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
                _navigationManager.NavigateTo($"/dang-ky-doi-ca?key={key}");
                ShiftRequestDocument = new ShiftChangeModel();
                ListShiftChange = new List<ShiftChange1Model>();
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

        protected async Task CancelDocumentHandler()
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
                errorMessage = string.Format(MessageConstants.MESSAGE_CONFIRM_SEND_APPROVAL_FORMAT, $"đến nhân viên {ShiftRequestDocument.employeeSignatureName}");
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{errorMessage}");
                if (!isConfirm) return;
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
