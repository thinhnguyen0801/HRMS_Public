using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using HNOne.Web.Models;
using HNOne.Web.Services;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HNOne.Web.Controllers
{
    public class SalaryPaymentController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] ISalaryService _salaryService { get; init; }
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
        public bool firstRender = true;

        public SalaryPaymentModel SalaryRequestDocument { get; set; } = new SalaryPaymentModel();
        public List<SalaryPayment1Model>? ListSalaryPayment { get; set; } // danh sách hạch toán chi phí lương
        public IGrid? GridSalaryPayment { get; set; }

        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public List<EnumCatagoryModel>? ListCboRequestType { get; set; } // cbo ds loại chi
        public List<EnumCatagoryModel>? ListCboPaymentType { get; set; } // cbo ds phương thức thanh toán
        public List<ComboboxModel>? ListCboPreiod { get; set; } // cbo ds kỳ lương
        public List<ComboboxModel>? ListCboAccouting { get; set; } // cbo ds tài khoản
        public string? VoucherHistory { get; set; } = string.Empty; // lịch sử chứng từ

        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public bool IsShowDialogEmpSearch { get; set; }
        public string? DepartmentIds { get; set; }
        public string? StatusIds { get; set; } // Tình trạng nào
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn


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
                        new BreadcrumbModel("Lương"),
                        new BreadcrumbModel("Hạch toán chi phí lương", "danh-sach-hach-toan-chi-phi-luong"),
                        new BreadcrumbModel("Chi tiết", isActive: true),
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
            SalaryRequestDocument.statusCode = CommonConstants.STATUS_CODE_ADD; // mặc định là chờ xử lý
            SalaryRequestDocument.paymentTypeCode = CommonConstants.ENUM_PAYMENT_TYPE_TM;
            SalaryRequestDocument.paymentRequestTypeCode = CommonConstants.ENUM_PAYMENT_REQUEST_TYPE_CHILUONG;
            SalaryRequestDocument.branchId = BranchId;
            SalaryRequestDocument.docDate = DateTime.Now;
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
                request.process = ProcessConstants.GET_SALARY_MASTER_DATA;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.type = ProcessConstants.GET_COMBO_LIST_SALARY_PREIOD;
                var getTask4 = _masterDataService.GetBranchAsync(UserId, Token);
                var getTask5 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiHopDong)); // ds trạng thái
                var getTask1 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.LoaiChiLuong)); // ds loại chi lương
                var getTask2 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.PhuongThucThanhToan)); // ds phương thức thanh toán
                var getTask3 = _salaryService.GetMasterDataAsync<ComboboxModel>(request);
                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask3,
                    getTask4,
                    getTask5
                );
                ListCboPreiod = await getTask3;
                ListCboStatus = await getTask5;
                ListCboRequestType = await getTask1;
                ListCboPaymentType = await getTask2;
                ListCboBranch = (await getTask4)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
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
                request.process = ProcessConstants.GET_SALARY_EXPENSE_ACCOUNTING;
                var task1 = _salaryService.GetMasterDataAsync<SalaryPaymentModel>(request);
                var task2 = getDocumentHistory();
                await Task.WhenAll(task1, task2);
                List<SalaryPaymentModel>? lstData = await task1;
                if (!lstData.IsNullOrEmpty())
                {
                    SalaryRequestDocument = lstData![0];
                    //cho phép chỉnh sữa khi tình trạng là: A (Tạo mới), Y (Đã gửi yêu cầu phê duyệt)
                    IsReadonlyControl = SalaryRequestDocument.statusCode != CommonConstants.STATUS_CODE_ADD
                        && SalaryRequestDocument.statusCode != CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                    if (!string.IsNullOrEmpty(SalaryRequestDocument.jsonDetail))
                    {
                        ListSalaryPayment = JsonConvert.DeserializeObject<List<SalaryPayment1Model>>(SalaryRequestDocument.jsonDetail);
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
            => VoucherHistory = await _approvalService.GetFunDocumentHistoryAsync(UserId, BranchId, Token, nameof(EnumObjType.SalaryExpenseAccountings), pDocEntry);

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (ListSalaryPayment.IsNullOrEmpty())
            {
                errorMessage = "Không tìm thấy danh sách chi tiết. Vui lòng làm mới danh sách chi tiết!!!";
                fieldName = "gridInfo";
                return;
            }
            //var itemCheck = ListSalaryExpenseAccounting!.FirstOrDefault(m => string.IsNullOrEmpty(m.account1) || string.IsNullOrEmpty(m.account2));
            //if (itemCheck != null)
            //{
            //    errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, $"Tài khoản cần hoạch toán cho chi phí {itemCheck.salaryCatagoryName}");
            //    fieldName = "gridInfo";
            //    return;
            //}
            if (string.IsNullOrEmpty(SalaryRequestDocument.salaryPreiod))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Kỳ lương");
                fieldName = "pShiftPreiodId";
                return;
            }
            if (SalaryRequestDocument.dueDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Ngày hạch toán");
                fieldName = "dueDate";
                return;
            }
            if (SalaryRequestDocument.employeeSignatureId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(SalaryRequestDocument.employeeSignatureId);
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
            if (SalaryRequestDocument.id < 1)
            {
                errorMessage = "Vui lòng lưu thông tin chứng từ trước khi gửi phê duyệt";
                fieldName = "zzzz";
                return;
            }
            if (SalaryRequestDocument.employeeSignatureId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(SalaryRequestDocument.employeeSignatureId);
                return;
            }
        }
        #endregion Protected Functions

        #region
        protected async Task OpenPopupHandler(string type = nameof(EmployeeSelected),
            string popupType = nameof(SalaryRequestDocument.employeeSignatureCode))
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
                    case nameof(SalaryRequestDocument.employeeSignatureCode):
                        SalaryRequestDocument.employeeSignatureId = employee.id;
                        SalaryRequestDocument.employeeSignatureCode = employee.code;
                        SalaryRequestDocument.employeeSignatureName = employee.name;
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
                string processKey = pActionType == nameof(EnumType.Add) ? ProcessConstants.POST_SALARY_EXPENSE_ACCOUNTING : ProcessConstants.PUT_SALARY_EXPENSE_ACCOUNTING;
                string[] arrDt = SalaryRequestDocument.salaryPreiod!.Split("-");
                SalaryRequestDocument.year = int.Parse(arrDt[0]);
                SalaryRequestDocument.month = int.Parse(arrDt[1]);
                SalaryRequestDocument.branchId = BranchId;
                SalaryRequestDocument.userSign = UserId;
                SalaryRequestDocument.userSign2 = UserId;
                string json = JsonConvert.SerializeObject(SalaryRequestDocument);
                string jsonDetail = JsonConvert.SerializeObject(ListSalaryPayment!);
                int result = await _salaryService.UpdateDocumentAsync(processKey, UserId, Token, BranchId, json, jsonDetail);
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
                errorMessage = string.Format(MessageConstants.MESSAGE_CONFIRM_SEND_APPROVAL_FORMAT, $"đến nhân viên {SalaryRequestDocument.employeeSignatureName}");
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{errorMessage}");
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = ProcessConstants.POST_APPROVAL;
                ApprovalModel approval = new ApprovalModel();
                approval.docEntry = SalaryRequestDocument.id;
                approval.objType = nameof(EnumObjType.SalaryExpenseAccountings);
                approval.branchId = BranchId;
                approval.statusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                approval.userSign = UserId;
                approval.employeeId = EmployeeId;
                approval.employeeSignatureId = SalaryRequestDocument.employeeSignatureId;
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
                _navigationManager.NavigateTo($"/chi-luong?key={key}");
                SalaryRequestDocument = new SalaryPaymentModel();
                ListSalaryPayment = new List<SalaryPayment1Model>();
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
        /// Thay đổi giá trị combobox
        /// </summary>
        /// <param name="value"></param>
        /// <param name="controlID"></param>
        /// <returns></returns>
        protected async Task ComboboxValueChangedHandler(object? value
            , string controlID = nameof(SalaryRequestDocument.salaryPreiod))
        {
            try
            {
                if (firstRender) return;
                switch (controlID)
                {
                    case nameof(SalaryRequestDocument.salaryPreiod):
                        SalaryRequestDocument.salaryPreiod = value?.ToString();
                        if (string.IsNullOrEmpty(SalaryRequestDocument.salaryPreiod)) break;
                        await ShowLoading();
                        await Task.Delay(75);
                        //await getAccountingSalaryType();
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

        protected async Task RefreshAccountingSalaryTypeHandler()
        {
            try
            {
                if (string.IsNullOrEmpty(SalaryRequestDocument.salaryPreiod))
                {
                    ShowWarning(string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Kỳ lương"));
                    await _jsRuntime.InvokeVoidAsync("focusInput", "pShiftPreiodId");
                    return;
                }
                await ShowLoading();
                await Task.Delay(75);
                //await getAccountingSalaryType();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "RefreshAccountingSalaryTypeHandler");
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
