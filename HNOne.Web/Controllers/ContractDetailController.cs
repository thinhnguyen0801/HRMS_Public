using HNOne.Web.Commons;
using HNOne.Model.Models;
using HNOne.Model;
using Microsoft.AspNetCore.Components;
using HNOne.Web.Services.Interfaces;
using HNOne.Web.Components.Controls;
using Microsoft.JSInterop;
using HNOne.Common;
using DevExpress.Blazor;
using HNOne.Web.Models;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using HNOne.Web.Services;
using System;

namespace HNOne.Web.Controllers
{
    public class ContractDetailController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IPersonnelService _personnelService { get; init; }
        [Inject] IApprovalService _approvalService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        const string STRING_KEY_EVENT_POST = "CONTRACT_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "CONTRACT_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "CONTRACT_CONTROLLER_DELETE";
        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public int ActiveTabIndex { get; set; } = 0;
        public ContractModel ContractDocument { get; set; } = new ContractModel();
        public List<SalaryConfigurationModel>? ListSalaryInfoConfig { get; set; } // danh sách thông tin lương
        public IGrid? GridSalaryInfoConfig { get; set; }

        public List<ContractAppendixModel>? ListContractAppendix { get; set; } // ds phụ lục theo hợp đồng
        public IGrid? GridContractAppendix { get; set; }

        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public List<ComboboxModel>? ListCboPosition { get; set; } // cbo ds chức vụ
        public List<ComboboxModel>? ListCboTitle { get; set; } // cbo ds chức danh
        public List<ContractTypeModel>? ListCboContractType { get; set; } // ds cbo loại hợp đồng
        public List<EnumCatagoryModel>? ListCboEnumTax { get; set; } // cbo ds loại tính thuế
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
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-hop-dong");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    this.firstRender = firstRender;
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Nhân sự"),
                        new BreadcrumbModel("Danh sách hợp đồng", "danh-sach-hop-dong"),
                        new BreadcrumbModel("Chi tiết hợp đồng", isActive: true),
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    initDataAsync();
                    await buildComboAsync();
                    if (pDocEntry < 1) await getSalaryConfigDefault();
                    else
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
            
            ContractDocument.salaryCoefficient = 1.0;
            ContractDocument.startDate = DateTime.Now;
            ContractDocument.numberOfMonths = 1;
            ContractDocument.contractNumber = 1;
            ContractDocument.numberOfDaysReduced = 1;
            ContractDocument.statusCode = CommonConstants.STATUS_CODE_ADD; // mặc định là chờ xử lý
            //ContractDocument.employeeId = EmployeeId;
            //ContractDocument.employeeCode = EmployeeCode;
            //ContractDocument.employeeName = EmployeeName;
            //ContractDocument.departmentId = DepartmentId;
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
                var getTask1 = _masterDataService.GetContractTypeAsync(UserId, Token, BranchId, opt: CommonConstants.ENUM_ACTIVE); // danh sách hợp đồng
                var getTask2 = _masterDataService.GetTitleAsync(UserId, Token, BranchId, opt: CommonConstants.ENUM_ACTIVE); // ds chức danh
                var getTask3 = _masterDataService.GetPositionAsync(UserId, Token, BranchId, opt: CommonConstants.ENUM_ACTIVE); // ds chức vụ
                var getTask4 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.DanhMucThueTNCN)); // ds loại tính thuế
                var getTask5 = _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiHopDong)); // ds trạng thái
                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask3,
                    getTask4,
                    getTask5
                );
                ListCboContractType = await getTask1;
                ListCboTitle = (await getTask2)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboPosition = (await getTask3)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboEnumTax = await getTask4;
                ListCboStatus = await getTask5;
            }
            catch (Exception) { throw; }
        }
        
        /// <summary>
        /// lấy danh sách cấu hình lương mặc định
        /// </summary>
        /// <returns></returns>
        private async Task getSalaryConfigDefault()
        {
            try
            {
                var lstSalaryConfig = await _masterDataService.GetSalaryConfigAsync(UserId, Token, BranchId, isShowToast: false);
                ListSalaryInfoConfig = lstSalaryConfig?.Update(m => m.amount = m.salaryDefault)?.ToList();
                calcTotalSalary();
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

        private async Task<string?> getDocumentNo(string? contractType) 
            => await _masterDataService.GetDocumentNo(UserId, Token, BranchId, GlobalContants.ENUM_CONTRACT_NO, contractType, ContractDocument.dateOfSigning.FormatDateTimeSql());

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if(ListSalaryInfoConfig.IsNullOrEmpty())
            {
                errorMessage = "Không tìm thấy thông tin cấu hình lương. Vui lòng làm mới lại trang!!!";
                fieldName = "gridSalary";
                return;
            }    
            if (ContractDocument.contractTypeId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Loại hợp đồng");
                fieldName = nameof(ContractDocument.contractTypeId);
                return;
            }
            if (string.IsNullOrWhiteSpace(ContractDocument.contractCode))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Mã hợp đồng");
                fieldName = nameof(ContractDocument.contractCode);
                return;
            }
            if (ContractDocument.employeeId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Nhân viên");
                fieldName = nameof(ContractDocument.employeeId);
                return;
            }
            if (ContractDocument.employeeSignatureId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Người ký");
                fieldName = nameof(ContractDocument.employeeSignatureId);
                return;
            }
            if (ContractDocument.positionId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chức vụ");
                fieldName = nameof(ContractDocument.positionId);
                return;
            }
            if (ContractDocument.titleId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chức danh");
                fieldName = nameof(ContractDocument.titleId);
                return;
            }
            if (ContractDocument.startDate == null)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Thời gian bắt đầu");
                fieldName = nameof(ContractDocument.startDate);
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
                var task1 = _personnelService.GetContractAsync(request);
                var task2 = getDocumentHistory();
                var task3 = getContractAppendixList();
                await Task.WhenAll(task1, task2);
                List<ContractModel>? lstData = await task1;
                if (!lstData.IsNullOrEmpty())
                {
                    ContractDocument = lstData![0];
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
        /// cập nhật ngày kết thúc
        /// </summary>
        private void calcEndDate()
        {
            ContractDocument.endDate = null;
            if(ContractDocument.startDate != null)
            {
                ContractDocument.endDate = ContractDocument.startDate.Value
                                .AddMonths((int)ContractDocument.numberOfMonths)
                                .AddDays(-1 * ContractDocument.numberOfDaysReduced);
            }    

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
                fieldName = nameof(ContractDocument.employeeSignatureId);
                return;
            }
        }
        
        /// <summary>
        /// lấy lịch sử chứng từ
        /// </summary>
        /// <returns></returns>
        private async Task getDocumentHistory()
            => VoucherHistory = await _approvalService.GetFunDocumentHistoryAsync(UserId, BranchId, Token, nameof(EnumObjType.Contracts), pDocEntry);

        /// <summary>
        /// lấy danh sách phụ lục hợp đồng theo hợp đồng
        /// </summary>
        /// <returns></returns>
        private async Task getContractAppendixList()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.branchId = BranchId;
            request.token = Token;
            request.opt = "";
            request.opt1 = pDocEntry.ToString();
            var lstContract = await _personnelService.GetContractAppendixAsync(request, isShowToast: false);
            lstContract = lstContract?.Update(m =>
            {
                Dictionary<string, string> pParams = new Dictionary<string, string>
                {
                    { "pActionType", nameof(EnumType.Update) },
                    { "pDocEntry", $"{m.id}" },
                    { "pContractId", $"{m.contractId}" },
                };
                m.link = "chi-tiet-phu-luc-hop-dong?key=" + _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
            })?.ToList();
            ListContractAppendix = lstContract;
        }
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
                        ContractDocument.employeeId = employee.id;
                        ContractDocument.employeeCode = employee.code;
                        ContractDocument.employeeName = employee.name;
                        ContractDocument.positionId = employee.positionId;
                        ContractDocument.titleId = employee.titleId ?? -1;
                        IsShowDialogEmpSearch = false;
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

        protected async Task ComboboxValueChangedHandler(object? value
            , string controlID = nameof(ContractDocument.contractTypeId), object? gridSelected = null)
        {
            try
            {
                if (firstRender) return;
                switch (controlID)
                {
                    case nameof(ContractDocument.contractTypeId):
                        var contractType = ListCboContractType?.FirstOrDefault(m => m.id == (int?)value);
                        if (contractType != null)
                        {
                            await ShowLoading();
                            ContractDocument.contractTypeId = contractType.id;
                            ContractDocument.contractNumber = 1;
                            ContractDocument.numberOfMonths = contractType.duration;
                            ContractDocument.numberOfDaysReduced = contractType.numberOfDaysReduced > 0 ? contractType.numberOfDaysReduced : 1;
                            ContractDocument.contractCode = await getDocumentNo(contractType.code); // gọi API lấy mã hợp đồng
                            await Task.Delay(100);
                            calcEndDate();
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
        /// thay đổi value cho control số
        /// </summary>
        /// <param name="value"></param>
        /// <param name="controlID"></param>
        /// <param name="isIndefiniteContract">HĐ không thời hạn ?</param>
        protected void SpinEditValueChangedHandler(double value
            , string controlID = nameof(ContractDocument.numberOfMonths), object? gridSelected = null)
        {
            try
            {
                if (firstRender) return;
                switch (controlID)
                {
                    case nameof(ContractDocument.numberOfMonths):
                        ContractDocument.numberOfMonths = value;
                        Task.Yield();
                        calcEndDate();
                        StateHasChanged();
                        break;
                    case nameof(ContractDocument.numberOfDaysReduced):
                        ContractDocument.numberOfDaysReduced = (int)value;
                        Task.Yield();
                        calcEndDate();
                        StateHasChanged();
                        break;

                    case nameof(ContractDocument.salaryCoefficient):
                        ContractDocument.salaryCoefficient = value;
                        calcTotalSalary();
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "SpinEditValueChangeHandler");
            }
        }

        /// <summary>
        /// thay đổi thông tin DateEdit
        /// </summary>
        /// <param name="value"></param>
        /// <param name="controlID"></param>
        /// <param name="gridSelected"></param>
        protected void DateEditValueChangedHandler(object? value
            , string controlID = nameof(ContractDocument.startDate), object? gridSelected = null)
        {
            try
            {
                if (firstRender) return;
                switch (controlID)
                {
                    case nameof(ContractDocument.startDate):
                        ContractDocument.endDate = null;
                        ContractDocument.startDate = (DateTime?)value;
                        Task.Yield();
                        calcEndDate();
                        StateHasChanged();
                        break;
                }    
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "SpinEditValueChangeHandler");
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
        /// làm mới mã hợp đồng
        /// </summary>
        /// <returns></returns>
        protected async Task RefreshContractNoHandler()
        {
            try
            {
                var contractType = ListCboContractType?.FirstOrDefault(m => m.id == ContractDocument.contractTypeId);
                if (contractType == null)
                {
                    ShowWarning(string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Loại hợp đồng"));
                    await _jsRuntime.InvokeVoidAsync("focusInput", nameof(ContractDocument.contractTypeId));
                    return;
                }   
                await ShowLoading();
                ContractDocument.contractCode = await getDocumentNo(contractType.code); // gọi API lấy mã hợp đồng
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
        
        /// <summary>
        /// lưu thông tin hợp đồng
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
                int check = 0; // 0 là thay đổi lương
                bool isConfirm = true;
                if(pActionType == nameof(EnumType.Add))
                {
                    RequestModel request = new RequestModel();
                    request.userId = UserId;
                    request.token = Token;
                    request.process = ProcessConstants.POST_CONTRACT;
                    request.employeeId = ContractDocument.employeeId;
                    request.type = ContractDocument.contractTypeId.ToString();
                    request.fromDate = ContractDocument.startDate;
                    var checkContract = await _personnelService.CheckDataAsync(request); // kiểm tra ông này có hợp đồng nào nữa không
                    if (checkContract?.status == StatusCodes.Status409Conflict)
                    {
                        errorMessage = checkContract.message;
                        await Task.Yield();
                        isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                        if (!isConfirm) return;
                        check = 1;
                    }
                }    
                if(check == 0)
                {
                    errorMessage = pActionType == nameof(EnumType.Add) ? MessageConstants.MESSAGE_CONFIRM_ADD : MessageConstants.MESSAGE_CONFIRM_UPDATE;
                    await Task.Yield();
                    isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                    if (!isConfirm) return;
                }    
                await ShowLoading();
                calcTotalSalary();
                string processKey = pActionType == nameof(EnumType.Add) ? ProcessConstants.POST_CONTRACT : ProcessConstants.PUT_CONTRACT;
                ContractDocument.branchId = BranchId;
                ContractDocument.userSign = UserId;
                ContractDocument.userSign2 = UserId;
                string json = JsonConvert.SerializeObject(ContractDocument);
                string jsonDetail = JsonConvert.SerializeObject(ListSalaryInfoConfig);
                int result = await _personnelService.UpdateContractAsync(processKey, UserId, Token, BranchId, json, jsonDetail);
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
                errorMessage = string.Format(MessageConstants.MESSAGE_CONFIRM_SEND_APPROVAL_FORMAT, $"đến nhân viên {ContractDocument.employeeSignatureName}");
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{errorMessage}");
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = ProcessConstants.POST_APPROVAL;
                ApprovalModel approval = new ApprovalModel();
                approval.docEntry = ContractDocument.id;
                approval.objType = nameof(EnumObjType.Contracts);
                approval.branchId = BranchId;
                approval.statusCode = CommonConstants.STATUS_CODE_APPROVAL_PENDING;
                approval.userSign = UserId;
                approval.employeeSignatureId = ContractDocument.employeeSignatureId;
                string content = JsonConvert.SerializeObject(approval);
                isConfirm = await _approvalService.UpdateApprovalAsync(processKey, UserId, Token, json: content);
                if(isConfirm) await showVoucher();
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
                _navigationManager.NavigateTo($"/chi-tiet-hop-dong?key={key}");
                ContractDocument = new ContractModel();
                ListSalaryInfoConfig = new List<SalaryConfigurationModel>();
                pActionType = nameof(EnumType.Add);
                pDocEntry = -1;
                VoucherHistory = string.Empty;
                initDataAsync(isRefresh: true);
                await buildComboAsync();
                await getSalaryConfigDefault();
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
        /// Tạo phụ lục hợp đồng
        /// </summary>
        /// <returns></returns>
        protected async Task CreateContractAppendixHandler()
        {
            try
            {
                if (ContractDocument.id < 1) return;
                Dictionary<string, string> pParams = new Dictionary<string, string>
                {
                    { "pActionType", $"{nameof(EnumType.Add)}" },
                    { "pDocEntry", $"{-1}" },
                    { "pContractId", $"{ContractDocument.id}" },
                    { "pEmployeeId", $"{ContractDocument.employeeId}" },
                    { "pIsPageContract", $"Y" }, // là tạo từ hợp đồng
                };
                string key = _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams)); // mã hóa key
                _navigationManager.NavigateTo($"/chi-tiet-phu-luc-hop-dong?key={key}");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "CreateContractAppendixHandler");
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// làm mới phụ lục hợp đồng
        /// </summary>
        /// <returns></returns>
        protected async Task RefreshDataAppendixHandler()
        {
            try
            {
                await ShowLoading();
                await getContractAppendixList();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "RefreshDataHandler");
            }
            finally
            {
                await Task.Delay(50);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        
        /// <summary>
        /// In hợp đồng
        /// </summary>
        /// <returns></returns>
        protected async Task PrintDocHandler()
        {
            try
            {
                await ShowLoading();
                var stream = await _masterDataService.PrintDocumentAsync(UserId, Token, BranchId, ContractDocument.id, ProcessConstants.GET_CONTRACT, "HDLD.docx");
                if (stream == null) return;
                await _jsRuntime.InvokeAsync<string>("downloadFileFromStream", "HDLD.docx", GlobalContants.MIME_TYPE_WORD, stream);
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
