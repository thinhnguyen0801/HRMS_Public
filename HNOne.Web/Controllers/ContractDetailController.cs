using HNOne.Web.Commons;
using HNOne.Model.Models;
using HNOne.Model;
using Microsoft.AspNetCore.Components;
using HNOne.Web.Services.Interfaces;
using HNOne.Web.Components.Controls;
using Microsoft.JSInterop;
using HNOne.Common;

namespace HNOne.Web.Controllers
{
    public class ContractDetailController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IPersonnelService _personnelService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public ContractModel ContractDocument { get; set; } = new ContractModel();
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public List<ComboboxModel>? ListCboPosition { get; set; } // cbo ds chức vụ
        public List<ComboboxModel>? ListCboTitle { get; set; } // cbo ds chức danh
        public List<ContractTypeModel>? ListCboContractType { get; set; } // ds cbo loại hợp đồng

        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public bool IsShowDialogEmpSearch { get; set; }
        public string? DepartmentIds { get; set; }
        public string? StatusIds { get; set; } // Tình trạng nào
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn
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
                            ContractDocument.contractTypeId = contractType.id;
                            ContractDocument.contractNumber = 1;
                            ContractDocument.contractCode = ""; // gọi API lấy mã hợp đồng
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
        #endregion
    }
}
