using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Models;
using HNOne.Web.Services;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace HNOne.Web.Controllers
{
    public class PersonalTimesheetController : DocumentControllerBase
    {
        [Inject] IPersonnelService _personnelService { get; init; }
        [Inject] IConfiguration _configuration { get; init; }
        [Inject] IWorkforceService _workforceService { get; init; }

        #region Properties
        public EmployeeModel EmployeeUpdate { get; set; } = new EmployeeModel();
        public ShiftAssignmentModel TimeSheet { get; set; } = new ShiftAssignmentModel();
        public List<ShiftAssignmentModel> ListDetail { get; set; } = new List<ShiftAssignmentModel>();
        public List<ComboboxModel>? ListCboPreiod { get; set; } // cbo ds kỳ công
        public string? pSalaryPreiodId { get; set; }
        public string? pSalaryPreiodVale { get; set; } // giá trị từ ngày đến ngày của kỳ công
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
                request.process = ProcessConstants.GET_WORKFORCE_MASTER_DATA;
                request.userId = UserId;
                request.branchId = BranchId;
                request.employeeId = EmployeeId;
                request.token = Token;
                request.type = ProcessConstants.GET_COMBO_LIST_SHIFT_PREIOD_BY_EMPLOYEE;
                ListCboPreiod = await _workforceService.GetMasterDataAsync<ComboboxModel>(request);
                if (!ListCboPreiod.IsNullOrEmpty())
                {
                    var first = ListCboPreiod!.First(); // cho xem bảng lương mới nhất
                    pSalaryPreiodId = first.code;
                    pSalaryPreiodVale = first.value;
                    await getAttendanceSummary();
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

        private async Task getAttendanceSummary()
        {
            if (EmployeeId < 1 || string.IsNullOrEmpty(pSalaryPreiodId)) return;
            TimeSheet = new ShiftAssignmentModel();
            ListDetail = new List<ShiftAssignmentModel>();
            string[] arrDt = pSalaryPreiodId!.Split("-");
            int.TryParse(arrDt[0], out int year);
            int.TryParse(arrDt[1], out int month);
            RequestModel request = new RequestModel();
            request.process = ProcessConstants.GET_ATTENDANCE_SUMMARY;
            request.userId = UserId;
            request.branchId = BranchId;
            request.token = Token;
            request.type = ProcessConstants.GET_TIME_SHEET_BY_EMPLOYEE;
            request.opt = year.ToString(); // năm
            request.opt1 = month.ToString(); // tháng
            request.departmentIds = "";
            request.opt2 = "";
            request.opt3 = $"{EmployeeId}";
            request.opt4 = "";
            var response = await _workforceService.GetMasterDataAsync<ShiftAssignmentModel>(request);
            if (!response.IsNullOrEmpty())
            {
                TimeSheet = response![0];
                ListDetail = response;
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
                        await getAttendanceSummary();
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
