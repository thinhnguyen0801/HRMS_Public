using DevExpress.Blazor;
using HNOne.Model;
using HNOne.Common;
using HNOne.Model.Models;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using HNOne.Web.Commons;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class AdjustedALRequestListController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IWorkforceService _workforceService { get; init; }
        const string STRING_KEY_EVENT_POST = "ADJUSTED_AL_REQUEST_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "ADJUSTED_AL_REQUEST_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "ADJUSTED_AL_REQUEST_CONTROLLER_DELETE";
        #region Properties
        public List<AdjustedAnnualLeaveRequestModel>? ListPending { get; set; }
        public IGrid? GridPending { get; set; }
        public List<AdjustedAnnualLeaveRequestModel>? ListAll { get; set; }
        public IGrid? GridAll { get; set; }
        public List<ComboboxModel>? ListCboYear { get; set; }
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public IReadOnlyList<object>? ListCboBranchSelected { get; set; } // chi nhánh được chọn
        public int ActiveTabIndex { get; set; } = 0;
        public int YearAdjusted { get; set; }
        public bool IsShowFilter { get; set; } = true; // mở rộng vùng tìm kiếm
        // nút quyền
        public bool IsAllowPost { get; set; }
        public bool IsAllowDelete { get; set; }
        public bool IsAllowPut { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-dieu-chinh-phep-nam");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Công - Phép", isActive: true),
                        new BreadcrumbModel("Danh sách điều chỉnh phép", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    YearAdjusted = DateTime.Now.Year;
                    await buildComboAsync();
                    await getAdjustedALRequestList();
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
        #endregion

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

        private async Task getAdjustedALRequestList()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.branchId = BranchId;
            request.token = Token;
            request.year = YearAdjusted;
            request.opt = ActiveTabIndex == 0 ? "ACTIVE" : ""; // tình trạng
            request.process = ProcessConstants.GET_ADJUSTED_ANNUAL_LEAVE_REQUEST;
            request.employeeId = EmployeeId;
            request.branchIds = ListCboBranchSelected.IsNullOrEmpty() ? BranchIds : string.Join(",", ListCboBranchSelected!.Cast<ComboboxModel>().Select(m => m.id));
            request.type = CommonConstants.ENUM_LIST;
            var listResult = await _workforceService.GetAdjustedALRequestAsync(request, isShowToast: true);
            listResult = listResult?.Update(m =>
            {
                Dictionary<string, string> pParams = new Dictionary<string, string>
                {
                    { "pActionType", nameof(EnumType.Update) },
                    { "pDocEntry", $"{m.id}" }
                };
                m.link = "dieu-chinh-phep-nam?key=" + _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
            })?.ToList();
            if (ActiveTabIndex == 0)
            {
                ListPending = listResult;
                return;
            }
            ListAll = listResult;
        }

        private async Task buildComboAsync()
        {
            try
            {
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_WORKFORCE_MASTER_DATA;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.type = ProcessConstants.GET_COMBO_ANNUAL_LEAVE_YEAR;
                var getTask4 = _masterDataService.GetBranchAsync(UserId, Token, BranchId, $"{BranchIds}", supperAdmin: IsAdmin ? "Y" : "N");
                var getTask3 = _workforceService.GetMasterDataAsync<ComboboxModel>(request, isShowToast: true);
                await Task.WhenAll(
                    getTask3,
                    getTask4
                );
                ListCboBranch = (await getTask4)?.Select(m => new ComboboxModel() { id = m.branchId, code = m.branchCode, name = m.branchName })?.ToList();
                ListCboYear = await getTask3;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "buildComboAsync");
            }
        }
        #endregion

        #region Protected Functions
        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getAdjustedALRequestList();
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

        protected async Task RedirectPageDetailHandler()
        {
            try
            {
                await checkPermission(MenuId);
                if (!IsAllowPost)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
                Dictionary<string, string> pParams = new Dictionary<string, string>
                {
                    { "pActionType", nameof(EnumType.Add) },
                    { "pDocEntry", "-1" },
                };
                string key = _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams)); // mã hóa key
                _navigationManager.NavigateTo($"/dieu-chinh-phep-nam?key={key}");
            }
            catch { }
        }

        /// <summary>
        /// Kết xuất dữ liệu sang file excel
        /// xlsx
        /// </summary>
        /// <returns></returns>
        protected async Task ExportExcelHandler()
        {
            try
            {
                if (ActiveTabIndex == 0)
                {
                    if (GridPending == null || ListPending.IsNullOrEmpty())
                    {
                        ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                        return;
                    }
                    await ShowLoading();
                    await GridPending!.ExportToXlsxAsync("danh-sach-dieu-chinh-phep-nam", new GridXlExportOptions()
                    {
                        ExportTotalSummaries = false,
                        ExportGroupSummaries = false
                    });
                    return;
                }
                if (GridAll == null || ListAll.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                await ShowLoading();
                await GridAll!.ExportToXlsxAsync("danh-sach-dieu-chinh-phep-nam", new GridXlExportOptions()
                {
                    ExportTotalSummaries = false,
                    ExportGroupSummaries = false
                });
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ExportExcelHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// Mở rộng & thu gọn vùng tìm kiếm
        /// </summary>
        protected void ShowFilterHandler() => IsShowFilter = !IsShowFilter;
        #endregion
    }
}
