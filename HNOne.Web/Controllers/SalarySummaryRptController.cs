using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model.Models;
using HNOne.Model;
using HNOne.Web.Commons;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using System.Data;
using HNOne.Web.Components.Controls;
using Newtonsoft.Json;
using HNOne.Web.Services;

namespace HNOne.Web.Controllers
{
    public class SalarySummaryRptController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }

        #region Properties
        public SearchModel SearchUpdate { get; set; } = new SearchModel();
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public List<ComboboxModel>? ListCboYear { get; set; }
        public List<ComboboxModel>? ListCboMonth { get; set; }
        #endregion

        #region Private Functions
        private void initDataAsync()
        {
            SearchUpdate.year = DateTime.Now.Year;
            SearchUpdate.month = DateTime.Now.Month;
            SearchUpdate.branchId = BranchId;
            int defaultYear = 2024;
            ListCboYear = new List<ComboboxModel>();
            for (int i = defaultYear; i < DateTime.Now.AddYears(2).Year; i++)
            {
                ListCboYear.Add(new ComboboxModel() { id = i, name = $"Năm {i}" });
            }

            ListCboMonth = new List<ComboboxModel>();
            for (int i = 1; i < 13; i++)
            {
                ListCboMonth.Add(new ComboboxModel() { id = i, name = $"Tháng {i}" });
            }
        }
        private async Task buildComboAsync()
        {
            try
            {
                var getTask1 = _masterDataService.GetDepartmentAsync(UserId, Token, BranchId, $"{BranchIds}", departmentIds: $"{DepartmentIds}", opt: CommonConstants.ENUM_FILTER); // ds phòng ban
                var getTask4 = _masterDataService.GetBranchAsync(UserId, Token, BranchId, $"{BranchIds}", supperAdmin: IsAdmin ? "Y" : "N");
                await Task.WhenAll(
                    getTask1,
                    getTask4
                );
                //ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, code = m.code, name = m.name })?.ToList();
                ListCboBranch = (await getTask4)?.Select(m => new ComboboxModel() { id = m.branchId, code = m.branchCode, name = m.branchName })?.ToList();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "buildComboAsync");
            }
        }
        #endregion
    }
}
