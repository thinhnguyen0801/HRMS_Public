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
using Microsoft.AspNetCore.Components.Forms;
using ClosedXML.Excel;
using System.Reflection;
using DevExpress.Data.ExpressionEditor;

namespace HNOne.Web.Controllers
{
    public class RewardAllowanceRequestController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] ISalaryService _salaryService { get; init; }
        [Inject] IApprovalService _approvalService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        const string STRING_KEY_EVENT_POST = "REWARD_ALLOWANCE_REQUEST_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "REWARD_ALLOWANCE_REQUEST_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "REWARD_ALLOWANCE_REQUEST_CONTROLLER_DELETE";
        const string STRING_KEY_EVENT_CANCEL = "REWARD_ALLOWANCE_REQUEST_CONTROLLER_CANCEL";
        const string STRING_KEY_EVENT_APPROVAL = "APPROVAL_CONTROLLER_PUT";

        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public int ActiveTabIndex { get; set; } = 0;
        public RewardAllowanceRequestModel RequestDocument { get; set; } = new RewardAllowanceRequestModel();
        public List<RewardAllowanceRequest1Model>? ListEmployeeReward { get; set; } // danh sách nhân viên được khen thưởng
        public IGrid? GridEmployeeReward { get; set; }

        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public List<SalaryConfigurationModel>? ListCboVoucherType { get; set; } // loại chế độ

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
        public bool IsAllowCancel { get; set; } // hủy phiếu để văn thư hay pns hủy
        public bool IsAllowApproval { get; set; }
        public bool IsShowPromptDeny { get; set; }
        public InputFile? inputFile { get; set; } // file
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-khen-thuong");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Lương"),
                        new BreadcrumbModel("Khen thưởng - Phụ cấp", "danh-sach-khen-thuong"),
                        new BreadcrumbModel("Chi tiết khen thưởng & phụ cấp", isActive: true),
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
            IsAllowCancel = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_CANCEL) != null;
        }

        private void initDataAsync(bool isRefresh = false)
        {
            // GÁN DỮ LIỆU MẶC ĐỊNH
            RequestDocument.statusCode = CommonConstants.STATUS_CODE_ADD; // mặc định là chờ xử lý
            RequestDocument.rewardDate = DateTime.Now;
            RequestDocument.rewardPaymentDate = DateTime.Now;
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
                var getTask1 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiHopDong)); // ds trạng thái
                var getTask2 = _masterDataService.GetSalaryConfigAsync(UserId, Token, BranchId, opt: CommonConstants.ENUM_ACTIVE
                    , allowanceType: CommonConstants.ENUM_ALLOWANCE_TYPE_KTPC, isShowToast: false); // ds cấu hình lương theo Khen thưởng & phụ cấp
                await Task.WhenAll(
                    getTask1,
                    getTask2
                );
                ListCboStatus = await getTask1;
                ListCboVoucherType = await getTask2;
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
            if (RequestDocument.salaryConfigId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Loại khen thưởng");
                fieldName = "salaryConfigId";
                return;
            }
            if (string.IsNullOrWhiteSpace(RequestDocument.rewardName))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Tên đợt thưởng");
                fieldName = "rewardName";
                return;
            }
            if (ListEmployeeReward.IsNullOrEmpty())
            {
                errorMessage = "Không tìm thấy danh sách nhân viên khen thưởng!!!";
                fieldName = "gridInfo";
                return;
            }
            // Kiểm tra dữ liệu lưới

            RewardAllowanceRequest1Model? itemCheck = ListEmployeeReward!.FirstOrDefault(m => m.paidAmount == 0);
            if (itemCheck != null)
            {
                errorMessage = $"Nhân viên [{itemCheck.employeeCode}]. Vui lòng điền số chi trả";
                fieldName = "gridInfo";
                return;
            }
            itemCheck = ListEmployeeReward!.FirstOrDefault(m => m.netSalary <= 0);
            if (itemCheck != null)
            {
                errorMessage = $"Nhân viên [{itemCheck.employeeCode}]. Số tiền thực lãnh không hợp lệ";
                fieldName = "gridInfo";
                return;
            }
        }

        /// <summary>
        /// Hiên thị thông tin chi tiết
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
                request.process = ProcessConstants.GET_REWARD_ALLOWANCE_REQUEST;
                var task1 = _salaryService.GetMasterDataAsync<RewardAllowanceRequestModel>(request);
                var task2 = getDocumentHistory();
                await Task.WhenAll(task1, task2);
                List<RewardAllowanceRequestModel>? lstData = await task1;
                if (!lstData.IsNullOrEmpty())
                {
                    RequestDocument = lstData![0];
                    //cho phép chỉnh sữa khi tình trạng là: A (Tạo mới), Y (Đã gửi yêu cầu phê duyệt)
                    IsReadonlyControl = RequestDocument.statusCode != CommonConstants.STATUS_CODE_ADD
                        && RequestDocument.statusCode != CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                    if (!string.IsNullOrEmpty(RequestDocument.jsonDetail))
                    {
                        ListEmployeeReward = JsonConvert.DeserializeObject<List<RewardAllowanceRequest1Model>>(RequestDocument.jsonDetail);
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
            => VoucherHistory = await _approvalService.GetFunDocumentHistoryAsync(UserId, BranchId, Token, nameof(EnumObjType.RewardAllowanceRequests), pDocEntry);
        
        /// <summary>
        /// Tính tổng tiền của nhân viên
        /// </summary>
        private void calculateTotal()
        {
            RequestDocument.totalReward = 0;
            if (ListEmployeeReward.IsNullOrEmpty()) return;
            decimal totalReward = 0;
            foreach(var item in ListEmployeeReward!)
            {
                item.totalSalary = item.paidAmount - item.taxPayment;
                item.netSalary = (item.maxSalary > 0 && item.totalSalary > item.maxSalary) ? item.maxSalary : item.totalSalary;
                totalReward += item.netSalary;
            }
            RequestDocument.totalReward = totalReward;
        }

        /// <summary>
        /// Lưu thông tin chứng từ
        /// </summary>
        /// <param name="isShowToast"></param>
        /// <returns></returns>
        private async Task<int> saveDocument(bool isShowToast = true)
        {
            try
            {
                calculateTotal();
                string processKey = pActionType == nameof(EnumType.Add) ? ProcessConstants.POST_REWARD_ALLOWANCE_REQUEST : ProcessConstants.PUT_REWARD_ALLOWANCE_REQUEST;
                RequestDocument.rewardDate = new DateTime(RequestDocument.rewardDate.Year, RequestDocument.rewardDate.Month, 01);
                RequestDocument.rewardPaymentDate = new DateTime(RequestDocument.rewardPaymentDate.Year, RequestDocument.rewardPaymentDate.Month, 01);
                RequestDocument.branchId = BranchId;
                RequestDocument.userSign = UserId;
                RequestDocument.userSign2 = UserId;
                string json = JsonConvert.SerializeObject(RequestDocument);
                string jsonDetail = JsonConvert.SerializeObject(ListEmployeeReward);
                int result = await _salaryService.UpdateDocumentAsync(processKey, UserId, Token, BranchId, json, jsonDetail, isShowToast: isShowToast);
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
                        objType = nameof(EnumObjType.RewardAllowanceRequests),
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

        /// <summary>
        /// đọc dữ liệu excel đổ vào lưới
        /// </summary>
        /// <param name="excelStream"></param>
        /// <returns></returns>
        private List<RewardAllowanceRequest1Model> readExcelToDataTable(Stream excelStream)
        {
            try
            {
                var list = new List<RewardAllowanceRequest1Model>();
                using (var workbook = new XLWorkbook(excelStream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var range = worksheet.RangeUsed();
                    var rows = range.RowsUsed();

                    var headerRow = rows.First();
                    var headers = headerRow.Cells().Select(c => c.GetString()).ToList();
                    // Map từ cột có dấu sang property
                    Dictionary<string, string> columnMap = new(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Mã nhân viên", "employeeCode" },
                        { "Tên nhân viên", "employeeName" },
                        { "Tiền thưởng", "rewardAmount" },
                        { "Tiền chi trả", "paidAmount" },
                        { "Tiền thuế", "taxPayment" },
                        { "Tổng tiền", "totalSalary" },
                        { "Giới hạn", "maxSalary" },
                        { "Thực lãnh", "netSalary" },
                        { "Ghi chú", "remark" },
                    };

                    foreach (var row in rows.Skip(1))
                    {
                        var obj = new RewardAllowanceRequest1Model();

                        for (int i = 0; i < headers.Count; i++)
                        {
                            string excelHeader = headers[i];
                            if (string.IsNullOrWhiteSpace(excelHeader)) continue;

                            // 1. Map nếu nằm trong dictionary
                            string propertyName = columnMap != null && columnMap.TryGetValue(excelHeader, out var mappedName)
                                ? mappedName
                                : excelHeader; // 2. Nếu không, dùng luôn header

                            PropertyInfo? prop = typeof(RewardAllowanceRequest1Model).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (prop != null && prop.CanWrite)
                            {
                                string? cellValue = row.Cell(i + 1).GetString()?.Trim();
                                try
                                {
                                    object? convertedValue = Convert.ChangeType(cellValue, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                                    prop.SetValue(obj, convertedValue);
                                }
                                catch
                                {
                                    // Option: log hoặc bỏ qua lỗi nếu không convert được
                                }
                            }
                        }

                        list.Add(obj);
                    }
                }
                return list;
            }
            catch (Exception) { throw; }
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
                        var salaryConfig = ListCboVoucherType?.FirstOrDefault(m => m.id == RequestDocument.salaryConfigId);
                        ListEmployeeReward ??= new List<RewardAllowanceRequest1Model>();
                        foreach (var item in ListEmpSelected!.Cast<EmployeeModel>())
                        {
                            if (ListEmployeeReward.Any(m => m.employeeCode == item.code)) continue;
                            var rewardAllowance = new RewardAllowanceRequest1Model();
                            rewardAllowance.employeeId = item.id;
                            rewardAllowance.employeeCode = item.code;
                            rewardAllowance.employeeName = item.name;
                            rewardAllowance.departmentId = item.departmentId;
                            rewardAllowance.departmentCode = item.departmentCode;
                            rewardAllowance.departmentName = item.departmentName;
                            rewardAllowance.salaryCalculateMethod = salaryConfig?.salaryCalculateMethod;
                            rewardAllowance.salaryCalculateMethodName = salaryConfig?.salaryCalculateMethodName;
                            rewardAllowance.rewardAmount = salaryConfig?.salaryDefault ?? 0; // số tiền mặc định
                            rewardAllowance.maxSalary = salaryConfig?.taxLimit ?? 0; // giới hạn tiền
                            ListEmployeeReward.Add(rewardAllowance);
                        }
                        GridEmployeeReward?.Reload();
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

        protected void GridRewardAllowanceEditSavingHandler(GridEditModelSavingEventArgs e)
        {
            try
            {
                if (IsReadonlyControl) return;
                var itemEdit = (RewardAllowanceRequest1Model)e.EditModel;
                var itemFind = ListEmployeeReward?.FirstOrDefault(m => m.rewardAllowanceId == itemEdit.rewardAllowanceId && m.employeeId == itemEdit.employeeId);
                if (itemFind == null) return;
                itemFind.remark = itemEdit.remark;
                itemFind.rewardAmount = itemEdit.rewardAmount;
                itemFind.paidAmount = itemEdit.paidAmount;
                itemFind.taxPayment = itemEdit.taxPayment;
                itemFind.maxSalary = itemEdit.maxSalary;
                itemFind.totalSalary = itemEdit.paidAmount - itemEdit.taxPayment;
                itemFind.netSalary = (itemEdit.maxSalary > 0 && itemFind.totalSalary > itemEdit.maxSalary) ? itemEdit.maxSalary : itemFind.totalSalary;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GridAdjustedEditSavingHandler");
            }
        }

        /// <summary>
        /// xóa dữ liệu ở lưới
        /// </summary>
        protected void DeleteDataHandler()
        {
            try
            {
                if (ListEmployeeReward.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                var lstSelected = GridEmployeeReward!.SelectedDataItems;
                if (lstSelected.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                foreach (RewardAllowanceRequest1Model item in lstSelected) ListEmployeeReward!.Remove(item);
                GridEmployeeReward?.Reload();
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
                    approval.objType = nameof(EnumObjType.RewardAllowanceRequests);
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
                RequestDocument = new RewardAllowanceRequestModel();
                ListEmployeeReward = new List<RewardAllowanceRequest1Model>();
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
                approval.objType = nameof(EnumObjType.RewardAllowanceRequests);
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
        /// fill dữ liệu theo loại khen thưởng
        /// Loại tính lương, tiền thưởng và giới hạn
        /// Nếu giới hạn có điền thì không fill
        /// </summary>
        /// <returns></returns>
        protected async Task FillSalaryByVoucherTypeHandler()
        {
            try
            {
                if (RequestDocument.salaryConfigId < 1)
                {
                    ShowWarning(string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Loại khen thưởng"));
                    await _jsRuntime.InvokeVoidAsync("focusInput", "salaryConfigId");
                    return;
                }
                if (ListEmployeeReward.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                var lstSelected = GridEmployeeReward!.SelectedDataItems;
                if (lstSelected.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"Điền giá trị cách tính phụ cấp, tiền thưởng và giới hạn (nếu có) <br /> cho các nhân viên được chọn" +
                    $". <br /> {MessageConstants.MESSAGE_CONFIRM_CONTINUE}");
                if (!isConfirm) return;
                var salaryConfig = ListCboVoucherType?.FirstOrDefault(m => m.id == RequestDocument.salaryConfigId);
                foreach(RewardAllowanceRequest1Model item in lstSelected)
                {
                    item.salaryCalculateMethod = salaryConfig?.salaryCalculateMethod;
                    item.salaryCalculateMethodName = salaryConfig?.salaryCalculateMethodName;
                    item.rewardAmount = salaryConfig?.salaryDefault ?? 0; // số tiền mặc định
                    decimal maxSalary = salaryConfig?.taxLimit ?? 0;
                    if (item.maxSalary <= 0 || maxSalary > 0) item.maxSalary = maxSalary; // nếu chưa có giới hạn hoặc giới hạn có ở mặc định thì lấy mặc định
                    item.totalSalary = item.paidAmount - item.taxPayment;
                    item.netSalary = (item.maxSalary > 0 && item.totalSalary > item.maxSalary)  ? item.maxSalary : item.totalSalary;
                }
                GridEmployeeReward!.Reload();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "CalculateSalaryHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        
        /// <summary>
        /// Tính lương nhân viên theo bảng công
        /// </summary>
        /// <returns></returns>
        protected async Task SalaryCalculateHandler()
        {
            try
            {
                if (ListEmployeeReward.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                var lstSelected = GridEmployeeReward!.SelectedDataItems;
                if (lstSelected.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"Tính lương nhân viên theo dữ liệu đã được khóa kỳ công tháng " +
                    $"{RequestDocument.rewardDate.Month} năm {RequestDocument.rewardDate.Year}. <br /> {MessageConstants.MESSAGE_CONFIRM_CONTINUE}");
                if (!isConfirm) return;
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "CalculateSalaryHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// Chọn cột hiển thị
        /// </summary>
        protected void ColumnChooseHandler()
        {
            try
            {
                if (GridEmployeeReward == null) return;
                GridEmployeeReward.ShowColumnChooser();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ExportExcelHandler");
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Import file
        /// </summary>
        /// <returns></returns>
        protected async Task ImportExcelHandler()
        {
            try
            {
                if (inputFile == null) return;
                await _jsRuntime.InvokeVoidAsync("triggerClick", inputFile.Element);
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ImportExcelHandler");
                ShowError(ex.Message);
            }
        }

        /// <summary>
        /// Import dữ liệu từ file excel
        /// </summary>
        /// <returns></returns>
        protected async Task OnLoadFileHandler(InputFileChangeEventArgs args)
        {
            try
            {
                if (args.FileCount <= 0) return;
                var lstFileExtension = args.GetMultipleFiles().Select(m => Path.GetExtension(m.Name));
                var checkExist = lstFileExtension.Any(m => m != ".xlsx");
                if (checkExist)
                {
                    ShowWarning("Bạn chỉ được phép đính kèm tệp excel(.xlsx)");
                    return;
                }
                var isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"Nhân viên sẽ được thay thế dữ liệu từ Excel <br /> Bạn có chắc muốn tiếp tục?");
                if (!isConfirm) return;
                await ShowLoading();
                using var stream = args.File.OpenReadStream();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                var result = readExcelToDataTable(memoryStream);
                if (result.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                ListEmployeeReward ??= new List<RewardAllowanceRequest1Model>();
                var lstEmp = new List<RewardAllowanceRequest1Model>();
                foreach (var item in result)
                {
                    if (string.IsNullOrEmpty(item.employeeCode)) continue;
                    var itemInGrid = ListEmployeeReward.FirstOrDefault(m => m.employeeCode == item.employeeCode);
                    if(itemInGrid != null)
                    {
                        // nếu trong lưới có ông nhân viên đó thì gán lại dữ liệu cho ổng
                        itemInGrid.remark = item.remark;
                        itemInGrid.rewardAmount = item.rewardAmount;
                        itemInGrid.paidAmount = item.paidAmount;
                        itemInGrid.taxPayment = item.taxPayment;
                        itemInGrid.maxSalary = item.maxSalary;
                        itemInGrid.totalSalary = item.paidAmount - item.taxPayment;
                        itemInGrid.netSalary = (item.maxSalary > 0 && itemInGrid.totalSalary > item.maxSalary) ? item.maxSalary : itemInGrid.totalSalary;
                        continue;
                    }
                    itemInGrid = new RewardAllowanceRequest1Model();
                    itemInGrid.employeeCode = item.employeeCode;
                    itemInGrid.employeeName = item.employeeName;
                    itemInGrid.remark = item.remark;
                    itemInGrid.rewardAmount = item.rewardAmount;
                    itemInGrid.paidAmount = item.paidAmount;
                    itemInGrid.taxPayment = item.taxPayment;
                    itemInGrid.maxSalary = item.maxSalary;
                    itemInGrid.totalSalary = item.paidAmount - item.taxPayment;
                    itemInGrid.netSalary = (item.maxSalary > 0 && itemInGrid.totalSalary > item.maxSalary) ? item.maxSalary : itemInGrid.totalSalary;
                    lstEmp.Add(itemInGrid);
                }
                ListEmployeeReward.AddRange(lstEmp);
                GridEmployeeReward?.Reload();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "ImportDataHandler");
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
