using DevExpress.Blazor;
using DevExpress.Pdf.Native.BouncyCastle.Asn1.Ocsp;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class PersonalPayslipController : DocumentControllerBase
    {
        [Inject] IPersonnelService _personnelService { get; init; }
        [Inject] ISalaryService _salaryService { get; init; }
        [Inject] IConfiguration _configuration { get; init; }
        #region Properties
        public EmployeeModel EmployeeUpdate { get; set; } = new EmployeeModel();
        public PayrollModel PayrollInfo { get; set; } = new PayrollModel();
        public List<SalaryConfigurationModel> ListAllowanceSalary { get; set; } = new List<SalaryConfigurationModel>();
        public List<SalaryConfigurationModel> ListRewardSalary { get; set; } = new List<SalaryConfigurationModel>(); // danh sách khen thưởng & phụ cấp
        public List<ComboboxModel>? ListCboPreiod { get; set; } // cbo ds kỳ lương
        public string? pSalaryPreiodId { get; set; }
        public string? pSalaryPreiodVale { get; set; } // giá trị từ ngày đến ngày của kỳ lương
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Phiếu lương nhân viên")
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await buildComboAsync();
                    await showVoucher();
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
        private async Task buildComboAsync()
        {
            try
            {
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_SALARY_MASTER_DATA;
                request.userId = UserId;
                request.branchId = BranchId;
                request.employeeId = EmployeeId;
                request.token = Token;
                request.type = ProcessConstants.GET_COMBO_LIST_SALARY_PREIOD_BY_EMPLOYEE;
                ListCboPreiod = await _salaryService.GetMasterDataAsync<ComboboxModel>(request);
                if (!ListCboPreiod.IsNullOrEmpty())
                {
                    var first = ListCboPreiod!.First(); // cho xem bảng lương mới nhất
                    pSalaryPreiodId = first.code;
                    pSalaryPreiodVale = first.value;
                    await getPayrollSummary();
                }
            }
            catch (Exception) { throw; }
        }
        private async Task showVoucher()
        {
            try
            {
                if (EmployeeId < 1) return;
                RequestModel request = new RequestModel();
                request.employeeId = EmployeeId;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                var lstData = await _personnelService.GetEmployeeAsync(request);
                if (!lstData.IsNullOrEmpty())
                {
                    EmployeeUpdate = lstData![0];
                    if (!string.IsNullOrEmpty(EmployeeUpdate.imageUrl))
                    {
                        string apiUrl = _configuration.GetSection("appSettings:ImageUrl").Value + "";
                        EmployeeUpdate.imageViewUrl = $"{apiUrl}{nameof(EmployeeController)}/{EmployeeUpdate.imageUrl}";
                    }
                }
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Lấy thông tin lương của nhân viên
        /// </summary>
        /// <returns></returns>
        private async Task getPayrollSummary()
        {
            if (EmployeeId < 1 || string.IsNullOrEmpty(pSalaryPreiodId)) return;
            PayrollInfo = new PayrollModel();
            ListAllowanceSalary = new List<SalaryConfigurationModel>();
            ListRewardSalary = new List<SalaryConfigurationModel>();
            string[] arrDt = pSalaryPreiodId!.Split("-");
            int.TryParse(arrDt[0], out int year);
            int.TryParse(arrDt[1], out int month);
            RequestModel request = new RequestModel();
            request.process = ProcessConstants.GET_PAYROLL_SUMMARY;
            request.userId = UserId;
            request.branchId = BranchId;
            request.token = Token;
            request.type = ProcessConstants.GET_SALARY_BY_EMPLOYEE;
            request.opt = year.ToString(); // năm
            request.opt1 = month.ToString(); // tháng
            request.departmentIds = "";
            request.opt2 = "";
            request.opt3 = $"{EmployeeId}";
            request.opt4 = "";
            var response = await _salaryService.GetMasterDataAsync<PayrollModel>(request);
            if (!response.IsNullOrEmpty())
            {
                PayrollInfo = response!.FirstOrDefault(m=>m.isRewardAllowance == false) ?? new PayrollModel();
                foreach(var item in response!)
                {
                    SalaryConfigurationModel itemAdd = new SalaryConfigurationModel();
                    itemAdd.salaryCategoryName = item.salaryCategoryName;
                    itemAdd.salaryCategoryCode = item.salaryCategoryCode;
                    itemAdd.amount = item.salaryAllowance;
                    if (item.isRewardAllowance == false)
                    {
                        ListAllowanceSalary.Add(itemAdd);
                    }
                    else
                    {
                        // Lương khen thưởng & phụ cấp khác
                        ListRewardSalary.Add(itemAdd);
                    }
                }
            }
        }
        #endregion

        #region Protected Functions
        /// <summary>
        /// Thay đổi giá trị combobox
        /// </summary>
        /// <param name="value"></param>
        /// <param name="controlID"></param>
        /// <returns></returns>
        protected async Task ComboboxValueChangedHandler(object? value
            , string controlID = nameof(pSalaryPreiodId))
        {
            try
            {
                switch (controlID)
                {
                    case nameof(pSalaryPreiodId):
                        pSalaryPreiodId = value?.ToString();
                        if (string.IsNullOrEmpty(pSalaryPreiodId)) return;
                        pSalaryPreiodVale = ListCboPreiod?.FirstOrDefault(m => m.code == pSalaryPreiodId)?.value;
                        await ShowLoading();
                        await Task.Delay(75);
                        await getPayrollSummary();
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
        #endregion
    }
}
