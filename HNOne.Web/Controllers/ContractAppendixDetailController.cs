using HNOne.Web.Components.Controls;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HNOne.Web.Services.Interfaces;
using HNOne.Web.Commons;
using DevExpress.Blazor;
using HNOne.Model.Models;
using HNOne.Model;
using HNOne.Web.Models;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using HNOne.Common;
using HNOne.Web.Services;
using static DevExpress.ReportServer.Printing.RemoteDocumentSource;
using Newtonsoft.Json.Linq;

namespace HNOne.Web.Controllers
{
    public class ContractAppendixDetailController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IPersonnelService _personnelService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        [Inject] IApprovalService _approvalService { get; init; }
        public W1Confirm confirm { get; set; }

        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        private int pContractId { get; set; } = 0; // id của hợp dồng
        public bool IsReadonlyControl { get; set; } = false;
        public int ActiveTabIndex { get; set; } = 0;
        public bool firstRender = true;
        public ContractAppendixModel ContractDocument { get; set; } = new ContractAppendixModel();
        public List<SalaryConfigurationModel>? ListSalaryInfoConfig { get; set; } // danh sách thông tin lương
        public IGrid? GridSalaryInfoConfig { get; set; }

        public List<ComboboxModel>? ListCboContract { get; set; } // cbo ds hợp hợp đồng của nhân viên
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public List<ComboboxModel>? ListCboPosition { get; set; } // cbo ds chức vụ
        public List<ComboboxModel>? ListCboTitle { get; set; } // cbo ds chức danh
        public List<EnumCatagoryModel>? ListCboEnumTax { get; set; } // cbo ds loại tính thuế
        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // cbo ds tình trạng

        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public bool IsShowDialogEmpSearch { get; set; }
        public string? DepartmentIds { get; set; }
        public string? StatusIds { get; set; } // Tình trạng nào
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn
        public string? VoucherHistory { get; set; } = string.Empty; // lịch sử chứng từ
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if(firstRender)
            {
                try
                {
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-phu-luc-hop-dong");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    this.firstRender = firstRender;
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Nhân sự"),
                        new BreadcrumbModel("Danh sách phụ lục hợp đồng", "danh-sach-phu-luc-hop-dong"),
                        new BreadcrumbModel("Chi tiết phụ lục hợp đồng", isActive: true),
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    //
                    await ShowLoading();
                    await initDataAsync();
                    await buildComboAsync();
                    if(pDocEntry > 0)
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
        private async Task initDataAsync(bool isRefresh = false)
        {
            
            //
            ContractDocument.salaryCoefficient = 1.0;
            ContractDocument.effectiveDate = DateTime.Now;
            ContractDocument.statusCode = CommonConstants.STATUS_CODE_ADD; // mặc định là chờ xử lý
            var uri = _navigationManager?.ToAbsoluteUri(_navigationManager.Uri);
            if (uri != null && QueryHelpers.ParseQuery(uri.Query).Count > 0)
            {
                string key = uri.Query.Substring(5); // bỏ ?key=
                Dictionary<string, string> pParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(_encryptHelper.Decrypt(key))!;
                if (pParams != null && pParams.Any())
                {
                    if (pParams.ContainsKey("pActionType")) pActionType = Convert.ToString(pParams["pActionType"]);
                    if (pParams.ContainsKey("pDocEntry")) pDocEntry = Convert.ToInt32(pParams["pDocEntry"]);
                    if (pParams.ContainsKey("pContractId")) pContractId = Convert.ToInt32(pParams["pContractId"]);
                    if (pParams.ContainsKey("pIsPageContract") 
                        && Convert.ToString(pParams["pIsPageContract"]) == "Y")
                    {
                        await getContractById(pContractId);
                    }    
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
                var getTask1 = _masterDataService.GetDepartmentAsync(UserId, Token); // ds chức danh
                var getTask2 = _masterDataService.GetTitleAsync(UserId, Token); // ds chức danh
                var getTask3 = _masterDataService.GetPositionAsync(UserId, Token); // ds chức vụ
                var getTask4 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.DanhMucThueTNCN)); // ds loại tính thuế
                var getTask5 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiHopDong)); // ds trạng thái
                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask3,
                    getTask4,
                    getTask5
                );
                ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboTitle = (await getTask2)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboPosition = (await getTask3)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboEnumTax = await getTask4;
                ListCboStatus = await getTask5;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// tính tiền lương
        /// </summary>
        private void calcTotalSalary()
        {
            ContractDocument.totalSalary = (ListSalaryInfoConfig?.Sum(m => m.amount) ?? 0);
            ContractDocument.netSalary = ContractDocument.totalSalary * (decimal)ContractDocument.salaryCoefficient;
            StateHasChanged();
        }

        /// <summary>
        /// hiển thị thông tin phiếu voucher
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
                request.opt1 = pContractId.ToString();
                var task1 = _personnelService.GetContractAppendixAsync(request, true);
                var task2 = getDocumentHistory();
                await Task.WhenAll(task1, task2);
                List<ContractAppendixModel>? lstData = await task1;
                if (!lstData.IsNullOrEmpty())
                {
                    ContractDocument = lstData![0];
                    ListCboContract = new List<ComboboxModel>() { new ComboboxModel() { id = ContractDocument.contractId, code = ContractDocument.contractCode, name = ContractDocument.contractCode} };
                    //cho phép chỉnh sữa khi tình trạng là: A (Tạo mới), Y (Đã gửi yêu cầu phê duyệt)
                    IsReadonlyControl = ContractDocument.statusCode != CommonConstants.STATUS_CODE_ADD
                        && ContractDocument.statusCode != CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                    if (!string.IsNullOrEmpty(ContractDocument.jsonDetail))
                    {
                        ListSalaryInfoConfig = JsonConvert.DeserializeObject<List<SalaryConfigurationModel>>(ContractDocument.jsonDetail);
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
            => VoucherHistory = await _approvalService.GetFunDocumentHistoryAsync(UserId, BranchId, Token, nameof(EnumObjType.ContractAppendices), pDocEntry);

        private void validateForSalaryAdjustment(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(ContractDocument.employeeCode))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Nhân viên");
                fieldName = nameof(ContractDocument.employeeCode);
                return;
            }
            if (string.IsNullOrEmpty(ContractDocument.contractCode))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Hợp đồng");
                fieldName = nameof(ContractDocument.contractCode);
                return;
            }
            if (string.IsNullOrEmpty(ContractDocument.employeeSignatureCode))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(ContractDocument.employeeSignatureCode);
                return;
            }
            if (string.IsNullOrWhiteSpace(ContractDocument.contractAppendixCode))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Số phục lục");
                fieldName = nameof(ContractDocument.contractAppendixCode);
                return;
            }
        }

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            validateForSalaryAdjustment(ref errorMessage, ref fieldName);
            if(string.IsNullOrEmpty(errorMessage) 
                && ContractDocument.isSalaryAdjustment)
            {
                if (ListSalaryInfoConfig.IsNullOrEmpty())
                {
                    errorMessage = "Không tìm thấy thông tin cấu hình lương. Vui lòng làm mới lại trang!!!";
                    fieldName = "gridSalary";
                    return;
                }
                if (string.IsNullOrEmpty(ContractDocument.taxTypeCode))
                {
                    errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Loại tính thuế TNCN");
                    fieldName = nameof(ContractDocument.taxTypeCode);
                    return;
                }
                if (ContractDocument.deductionDate == null)
                {
                    errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Ngày bắt đầu trích nộp");
                    fieldName = nameof(ContractDocument.deductionDate);
                    return;
                }
                if (ContractDocument.deductionDate.Value.Day != 1)
                {
                    errorMessage = $"[Ngày bắt đầu trích nộp] phải là ngày đầu tháng";
                    fieldName = nameof(ContractDocument.deductionDate);
                    return;
                }
            }    
        }

        /// <summary>
        /// lấy danh sách hợp đồng của nhân viên được chọn
        /// </summary>
        /// <returns></returns>
        private async Task getContractByEmpId(int emloyeeId)
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.token = Token;
            request.branchId = BranchId;
            request.type = ProcessConstants.GET_COMBO_TYPE_CONTRACT_BY_EMPLOYEEID;
            request.opt = emloyeeId.ToString();
            ListCboContract = new List<ComboboxModel>();
            ListCboContract = await _masterDataService.GetMasterDataAsync<ComboboxModel>(request);
        }

        /// <summary>
        /// lấy chi tiết hợp đồng
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns></returns>
        private async Task getContractById(int contractId)
        {
            try
            {
                RequestModel request = new RequestModel();
                request.documentId = contractId;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                var lstData = await _personnelService.GetContractAsync(request);
                if (!lstData.IsNullOrEmpty())
                {
                    var contract = lstData![0];
                    await getContractByEmpId(contract.employeeId);
                    ContractDocument.contractId = contract.id;
                    ContractDocument.employeeId = contract.employeeId;
                    ContractDocument.employeeCode = contract.employeeCode;
                    ContractDocument.employeeName = contract.employeeName;
                    ContractDocument.employeeSignatureId = contract.employeeSignatureId;
                    ContractDocument.employeeSignatureCode = contract.employeeSignatureCode;
                    ContractDocument.employeeSignatureName = contract.employeeSignatureName;
                    ContractDocument.departmentId = contract.departmentId;
                    ContractDocument.positionId = contract.positionId;
                    ContractDocument.titleId = contract.titleId;
                    ContractDocument.contractCode = contract.contractCode;
                    ContractDocument.contractAppendixCode = await getDocumentNo();
                    var contractNum = ListCboContract?.FirstOrDefault(m => m.code == ContractDocument.contractCode)?.value;
                    int.TryParse(contractNum, out int contractNumber);
                    ContractDocument.contractNumber = contractNumber < 1 ? 1 : contractNumber; // lấy số hợp đồng
                }    
            }
            catch(Exception){ throw; }
        }

        /// <summary>
        /// kiểm tra dữ liệu trươc khi gửi phê duyệt
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="fieldName"></param>
        private void validateForSaveApproval(ref string errorMessage, ref string fieldName)
        {
            if (ContractDocument.id < 1)
            {
                errorMessage = "Vui lòng lưu thông tin chứng từ trước khi gửi phê duyệt";
                fieldName = "zzzz";
                return;
            }
            if (ContractDocument.employeeSignatureId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(ContractDocument.employeeSignatureCode);
                return;
            }
        }

        private async Task<string?> getDocumentNo()
            => await _masterDataService.GetDocumentNo(UserId, Token, BranchId, GlobalContants.CONTRACT_APPENDIX_NO, "", ContractDocument.effectiveDate.FormatDateTimeSql());
        #endregion

        #region Protected Functions
        protected async Task OpenPopupHandler(string type = nameof(EmployeeSelected), string popupType = nameof(ContractDocument.employeeCode))
        {
            try
            {
                pPopupType = popupType;
                switch (type)
                {
                    case nameof(EmployeeSelected):
                        ListCboDepartment ??= new();
                        DepartmentIds = string.Join(",", ListCboDepartment.Select(m => m.id));
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
                    case nameof(ContractDocument.employeeCode):
                        // lấy danh sách hợp đồng theo nhân viên
                        await ShowLoading();
                        await Task.Delay(75);
                        await getContractByEmpId(employee.id); // lấy hợp đồng
                        ContractDocument.employeeId = employee.id;
                        ContractDocument.employeeCode = employee.code;
                        ContractDocument.employeeName = employee.name;
                        ContractDocument.departmentId = employee.departmentId;
                        ContractDocument.positionId = employee.positionId;
                        ContractDocument.titleId = employee.titleId ?? -1;
                        IsShowDialogEmpSearch = false;
                        if (ListCboContract.IsNullOrEmpty())
                        {
                            ShowWarning($"Nhân viên {ContractDocument.employeeName} hiện chưa có hợp đồng, nên không thể tạo phụ lục hợp đồng.");
                            break;
                        }
                        ContractDocument.contractCode = ListCboContract![0].code;
                        var contractNum = ListCboContract!.FirstOrDefault(m => m.code == ContractDocument.contractCode)?.value;
                        int.TryParse(contractNum, out int contractNumber);
                        ContractDocument.contractNumber = contractNumber < 1 ? 1 : contractNumber; // lấy số hợp đồng
                        ContractDocument.contractAppendixCode = await getDocumentNo(); // đánh mã hợp đồng
                        ListSalaryInfoConfig = new List<SalaryConfigurationModel>();
                        ContractDocument.isSalaryAdjustment = false;
                        break;
                    case nameof(ContractDocument.employeeSignatureCode):
                        ContractDocument.employeeSignatureId = employee.id;
                        ContractDocument.employeeSignatureCode = employee.code;
                        ContractDocument.employeeSignatureName = employee.name;
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
        /// lưu thông tin phụ lục hợp đồng
        /// </summary>
        /// <returns></returns>
        protected async Task SaveDataHandler()
        {
            try
            {
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
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = pActionType == nameof(EnumType.Add) ? ProcessConstants.POST_CONTRACT_APPENDIX : ProcessConstants.PUT_CONTRACT_APPENDIX;
                int contractId = ListCboContract?.FirstOrDefault(m => m.code == ContractDocument.contractCode)?.id ?? -1;
                calcTotalSalary();
                ContractDocument.contractId = contractId;
                ContractDocument.branchId = BranchId;
                ContractDocument.userSign = UserId;
                ContractDocument.userSign2 = UserId;
                string json = JsonConvert.SerializeObject(ContractDocument);
                string jsonDetail = string.Empty;
                if (ContractDocument.isSalaryAdjustment) jsonDetail = JsonConvert.SerializeObject(ListSalaryInfoConfig);
                int result = await _personnelService.UpdateContractAsync(processKey, UserId, Token, BranchId, json, jsonDetail);
                if (result > 0)
                {
                    pActionType = nameof(EnumType.Update);
                    pDocEntry = result;
                    pContractId = contractId;
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
        /// check điều chỉnh lương
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected async Task SalaryAdjustmentCheckedChangedHandler(bool value)
        {
            try
            {
                if (firstRender) return;
                if (!value)
                {
                    ContractDocument.isSalaryAdjustment = value;
                    return;
                }
                string errorMessage = string.Empty;
                string fieldName = string.Empty; // trả ra trường nào cần validate
                validateForSalaryAdjustment(ref errorMessage, ref fieldName);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                    return;
                }
                await ShowLoading();
                await Task.Delay(75);
                ContractDocument.salaryCoefficient = 0;
                ContractDocument.totalSalary = 0;
                ContractDocument.netSalary = 0;
                ContractDocument.isSalaryAdjustment = value;
                int contractId = ListCboContract?.FirstOrDefault(m => m.code == ContractDocument.contractCode)?.id ?? -1;
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.token = Token;
                request.branchId = BranchId;
                request.type = ProcessConstants.GET_COMBO_TYPE_SALARY_ADJUSTMENT_BY_CONTRACT;
                request.opt = contractId.ToString();
                ListSalaryInfoConfig = new List<SalaryConfigurationModel>();
                ListSalaryInfoConfig = await _masterDataService.GetMasterDataAsync<SalaryConfigurationModel>(request, isShowToast: true);
                if(!ListSalaryInfoConfig.IsNullOrEmpty())
                {
                    ContractDocument.salaryCoefficient = ListSalaryInfoConfig![0].overtimeCoefficient;
                    calcTotalSalary();
                }    
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "SalaryAdjustmentCheckedChangedHandler");
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task ComboboxValueChangedHandler(object? value
            , string controlID = nameof(ContractDocument.contractCode))
        {
            try
            {
                if (firstRender) return;
                switch (controlID)
                {
                    case nameof(ContractDocument.contractCode):
                        await ShowLoading();
                        await Task.Delay(75);
                        ContractDocument.contractCode = value?.ToString();
                        var contractNum = ListCboContract?.FirstOrDefault(m => m.code == ContractDocument.contractCode)?.value;
                        int.TryParse(contractNum, out int contractNumber);
                        ContractDocument.contractNumber = contractNumber < 1 ? 1 : contractNumber; // lấy số hợp đồng
                        ContractDocument.contractAppendixCode = await getDocumentNo();
                        ListSalaryInfoConfig = new List<SalaryConfigurationModel>();
                        ContractDocument.isSalaryAdjustment = false;
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

        protected void GridSalaryInfoConfigEditSavingHandler(GridEditModelSavingEventArgs e)
        {
            var itemEdit = (SalaryConfigurationModel)e.EditModel;
            var itemFind = ListSalaryInfoConfig?.FirstOrDefault(m => m.id == itemEdit.id);
            if (itemFind == null) return;
            itemFind.amount = itemEdit.amount;
            calcTotalSalary();
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
                errorMessage = string.Format(MessageConstants.MESSAGE_CONFIRM_SEND_APPROVAL_FORMAT, $"đến nhân viên {ContractDocument.employeeSignatureName}");
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{errorMessage}");
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = ProcessConstants.POST_APPROVAL;
                ApprovalModel approval = new ApprovalModel();
                approval.docEntry = ContractDocument.id;
                approval.objType = nameof(EnumObjType.ContractAppendices);
                approval.branchId = BranchId;
                approval.statusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                approval.userSign = UserId;
                approval.employeeSignatureId = ContractDocument.employeeSignatureId;
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
        /// làm mới mã hợp đồng
        /// </summary>
        /// <returns></returns>
        protected async Task RefreshContractAppendixNoHandler()
        {
            try
            {
                await ShowLoading();
                ContractDocument.contractAppendixCode = await getDocumentNo();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "OpenPopupHandler");
            }
            finally
            {
                await Task.Delay(100);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion
    }
}
