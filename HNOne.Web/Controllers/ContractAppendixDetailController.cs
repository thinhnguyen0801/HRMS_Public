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

namespace HNOne.Web.Controllers
{
    public class ContractAppendixDetailController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IPersonnelService _personnelService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public bool IsReadonlyControl { get; set; } = false;
        public ContractAppendixModel ContractDocument { get; set; } = new ContractAppendixModel();
        public List<SalaryConfigurationModel>? ListSalaryInfoConfig { get; set; } // danh sách thông tin lương
        public IGrid? GridSalaryInfoConfig { get; set; }

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
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if(firstRender)
            {
                try
                {
                    await ShowLoading();
                    await initDataAsync();
                    await buildComboAsync();
                    if(pDocEntry > 0)
                    {
                        //await showVoucher();
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
        private async Task initDataAsync(bool isRefresh = false)
        {
            ListBreadcrumbs = new List<BreadcrumbModel>()
            {
                new BreadcrumbModel("Nhân sự"),
                new BreadcrumbModel("Danh sách phụ lục hợp đồng", "danh-sach-phu-luc-hop-dong"),
                new BreadcrumbModel("Chi tiết phụ lục hợp đồng", isActive: true),
            };
            await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
            //
            ContractDocument.salaryCoefficient = 1.0;
            ContractDocument.dateOfSigning = DateTime.Now;
            ContractDocument.statusCode = "1"; // mặc định là chờ xử lý
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
                var getTask1 = _masterDataService.GetDepartmentAsync(UserId, Token); // ds chức danh
                var getTask2 = _masterDataService.GetTitleAsync(UserId, Token); // ds chức danh
                var getTask3 = _masterDataService.GetPositionAsync(UserId, Token); // ds chức vụ
                var getTask4 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.DanhMucThueTNCN)); // ds loại tính thuế
                var getTask5 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiHopDong)); // ds trạng thái
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

        private async Task showVoucher()
        {
            try
            {
                RequestModel request = new RequestModel();
                request.documentId = pDocEntry;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                //var lstData = await _personnelService.GetContractAsync(request);
                //if (!lstData.IsNullOrEmpty())
                //{
                //    ContractDocument = lstData![0];
                //    if (!string.IsNullOrEmpty(ContractDocument.jsonDetail))
                //    {
                //        ListSalaryInfoConfig = JsonConvert.DeserializeObject<List<SalaryConfigurationModel>>(ContractDocument.jsonDetail);
                //        GridSalaryInfoConfig?.Reload();
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                        ContractDocument.departmentId = employee.departmentId;
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

        protected async Task SaveDataHandler()
        {

        }
        #endregion
    }
}
