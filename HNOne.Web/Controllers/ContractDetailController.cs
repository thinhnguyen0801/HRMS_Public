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
using static DevExpress.ReportServer.Printing.RemoteDocumentSource;
using Newtonsoft.Json.Linq;
using HNOne.Model.Entities;

namespace HNOne.Web.Controllers
{
    public class ContractDetailController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IPersonnelService _personnelService { get; init; }
        [Inject] IEncryptHelper _encryptHelper { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public int ActiveTabIndex { get; set; } = 0;
        public bool IsReadonlyControl { get; set; } = false;
        public ContractModel ContractDocument { get; set; } = new ContractModel();
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public List<ComboboxModel>? ListCboPosition { get; set; } // cbo ds chức vụ
        public List<ComboboxModel>? ListCboTitle { get; set; } // cbo ds chức danh
        public List<ContractTypeModel>? ListCboContractType { get; set; } // ds cbo loại hợp đồng
        public List<EnumCatagoryModel>? ListCboEnumTax { get; set; } // cbo ds loại tính thuế
        public List<SalaryConfigurationModel>? ListSalaryInfoConfig { get; set; } // danh sách thông tin lương
        public IGrid? GridSalaryInfoConfig { get; set; }

        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public bool IsShowDialogEmpSearch { get; set; }
        public string? DepartmentIds { get; set; }
        public string? StatusIds { get; set; } // Tình trạng nào
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await ShowLoading();
                    await initDataAsync();
                    await buildComboAsync();
                    if (ContractDocument.id < 1) await getSalaryConfigDefault();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OnAfterRenderAsync");
                    ShowError(ex.Message);
                }
                finally
                {
                    await ShowLoading(false);
                    //await _progressService!.Done();
                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        #region Private Functions
        private async Task initDataAsync(bool isRefresh = false)
        {
            ListBreadcrumbs = new List<BreadcrumbModel>()
            {
                new BreadcrumbModel("Nhân sự"),
                new BreadcrumbModel("Danh sách hợp đồng", "danh-sach-hop-dong"),
                new BreadcrumbModel("Chi tiết hợp đồng", isActive: true),
            };
            await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);

            //
            ContractDocument.salaryCoefficient = 1.0;
            ContractDocument.startDate = DateTime.Now;
            ContractDocument.dateOfSigning = DateTime.Now;
            ContractDocument.numberOfMonths = 1;
            ContractDocument.contractNumber = 1;
            ContractDocument.numberOfDaysReduced = 1;
            var uri = _navigationManager?.ToAbsoluteUri(_navigationManager.Uri);
            if (uri != null && QueryHelpers.ParseQuery(uri.Query).Count > 0)
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
                var getTask1 = _masterDataService.GetContractTypeAsync(UserId, Token); // danh sách hợp đồng
                var getTask2 = _masterDataService.GetTitleAsync(UserId, Token); // ds chức danh
                var getTask3 = _masterDataService.GetPositionAsync(UserId, Token); // ds chức vụ
                var getTask4 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.DanhMucThueTNCN)); // ds loại tính thuế
                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask3,
                    getTask4
                );
                ListCboContractType = await getTask1;
                ListCboTitle = (await getTask2)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboPosition = (await getTask3)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
                ListCboEnumTax = await getTask4;
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
                var lstSalaryConfig = await _masterDataService.GetSalaryConfigAsync(UserId, Token, isShowToast: false);
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

        protected async void ComboboxValueChangedHandler(object? value
            , string controlID = nameof(ContractDocument.contractTypeId), object? gridSelected = null)
        {
            try
            {
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
                switch (controlID)
                {
                    case nameof(ContractDocument.numberOfMonths):
                        // kiểm xem có phải HĐ không thời hạn không
                        bool isIndefiniteContract = ListCboContractType?
                            .FirstOrDefault(m => m.id == ContractDocument.contractTypeId)?.isIndefiniteDuration ?? false;
                        ContractDocument.numberOfMonths = value;
                        ContractDocument.endDate = null;
                        if (!isIndefiniteContract && ContractDocument.startDate != null)
                        {
                            ContractDocument.endDate = ContractDocument.startDate.Value
                                .AddMonths((int)ContractDocument.numberOfMonths)
                                .AddDays(-1 * ContractDocument.numberOfDaysReduced);
                        }
                        StateHasChanged();
                        break;

                    //case nameof(ContractDocument.co):
                    //    ContractDocument.heSoLuong = value;
                    //    calcTotalSalary();
                    //    break;
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
        #endregion
    }
}
