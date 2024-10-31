using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using HNOne.Web.Models;

namespace HNOne.Web.Controllers
{
    public class EmployeeListController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IPersonnelService _personnelService { get; init; }
        [Inject] IEncryptHelper _encryptHelper { get; init; }
        #region Properties
        public string? StatusFilter { get; set; } // tình trạng
        public List<EmployeeModel>? ListEmployee { get; set; }
        public IGrid? GridEmployee { get; set; }
        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // danh sách tình trạng nhân viên
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
                        new BreadcrumbModel("Nhân sự", isActive: true),
                        new BreadcrumbModel("Danh sách nhân viên", isActive: true)
                    };
                    //string errMessage = await CheckAuthMenuAsync("contractlist");
                    //if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    //Permission = await _masterDataService.GetAccessControl(UserId, Token, BranchId, 10012);
                    //ItemSearch.fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    //ItemSearch.toDate = DateTime.Now;
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await buildComboAsync();
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
        private async Task buildComboAsync()
        {
            try
            {

                ListCboStatus = await _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiNhanVien)); // ds trạng thái nhân viên
                if (!ListCboStatus.IsNullOrEmpty()) StatusFilter = ListCboStatus![0].code;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "buildComboAsync");
            }
        }
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

        #region Protected Functions
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

        protected void RedirectPageDetailHandler()
        {
            try
            {
                Dictionary<string, string> pParams = new Dictionary<string, string>
                {
                    { "pActionType", nameof(EnumType.Add) },
                    { "pDocEntry", "-1" },
                };
                string key = _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams)); // mã hóa key
                _navigationManager.NavigateTo($"/ho-so-nhan-vien?key={key}");
            }
            catch { }
        }
        #endregion

    }
}
