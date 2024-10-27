using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;

namespace HNOne.Web.Controllers
{
    public class EmployeeListController : DocumentControllerBase
    {
        [Inject] IPersonnelService _personnelService { get; init; }
        [Inject] IEncryptHelper _encryptHelper { get; init; }
        #region Properties
        public List<EmployeeModel>? ListEmployee { get; set; }
        public IGrid? GridEmployee { get; set; }
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await ShowLoading();
                    //string errMessage = await CheckAuthMenuAsync("contractlist");
                    //if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    //Permission = await _masterDataService.GetAccessControl(UserId, Token, BranchId, 10012);
                    //ItemSearch.fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    //ItemSearch.toDate = DateTime.Now;
                    //await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await getEmployee();
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
        private async Task getEmployee()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.branchId = BranchId;
            request.opt = "";
            ListEmployee = new List<EmployeeModel>();
            var lstEmp = await _personnelService.GetEmployeeAsync(request);
            ListEmployee = lstEmp?.Update(m =>
            {
                Dictionary<string, string> pParams = new Dictionary<string, string>
                {
                    { "pActionType", nameof(EnumType.Update) },
                    { "pDocEntry", $"{m.id}" },
                };
                m.link = _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
            })?.ToList();
        }
        #endregion

        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getEmployee();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ReLoadDataHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(50);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
