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
        const string STRING_KEY_EVENT_POST = "EMPLOYEE_CONTROLLER_POST";
        const string STRING_KEY_EVENT_DELETE = "EMPLOYEE_CONTROLLER_DELETE";
        const string STRING_KEY_EVENT_PUT = "EMPLOYEE_CONTROLLER_PUT";
        #region Properties
        public string? StatusFilter { get; set; } // tình trạng
        public List<EmployeeModel>? ListEmployee { get; set; }
        public IGrid? GridEmployee { get; set; }
        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // danh sách tình trạng nhân viên

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
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-nhan-vien");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Nhân sự", isActive: true),
                        new BreadcrumbModel("Danh sách nhân viên", isActive: true)
                    };
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

        /// <summary>
        /// kiểm tra quyền nút
        /// </summary>
        /// <returns></returns>
        private async Task checkPermission(string menuId)
        {
            //List<string> lstKey = await CheckEventPermission(menuId);
            //IsAllowPost = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_POST) != null;
            //IsAllowDelete = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_DELETE) != null;
            //IsAllowPut = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_PUT) != null;
            IsAllowPost = true;
            IsAllowDelete = true;
            IsAllowPut = true;
        }

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
            if(IsAllowPut)
            {
                ListEmployee = lstEmp?.Update(m =>
                {
                    Dictionary<string, string> pParams = new Dictionary<string, string>
                    {
                        { "pActionType", nameof(EnumType.Update) },
                        { "pDocEntry", $"{m.id}" },
                    };
                    m.link = "ho-so-nhan-vien?key=" + _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
                })?.ToList();
            }
            else
            {
                ListEmployee = lstEmp;
            }
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
