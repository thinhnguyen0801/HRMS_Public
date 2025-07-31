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
    public class RewardAllowanceRequestListController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IWorkforceService _workforceService { get; init; }
        [Inject] ISalaryService _salaryService { get; init; }

        const string STRING_KEY_EVENT_POST = "REWARD_ALLOWANCE_REQUEST_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "REWARD_ALLOWANCE_REQUEST_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "REWARD_ALLOWANCE_REQUEST_CONTROLLER_DELETE";

        #region Properties
        public List<RewardAllowanceRequestModel>? ListPending { get; set; }
        public IGrid? GridPending { get; set; }
        public List<RewardAllowanceRequestModel>? ListAll { get; set; }
        public IGrid? GridAll { get; set; }
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public IReadOnlyList<object>? ListCboBranchSelected { get; set; } // chi nhánh được chọn

        public int ActiveTabIndex { get; set; } = 0;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IsShowFilter { get; set; } = true; // mở rộng vùng tìm kiếm
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
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-khen-thuong");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Lương", isActive: true),
                        new BreadcrumbModel("Danh sách khen thưởng - phụ cấp", isActive: false)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01);
                    ToDate = DateTime.Now;

                    await buildComboAsync();
                    await getRewardRequestList();
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

        #region
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

        /// <summary>
        /// lấy danh sách khen thưởng - phụ cấp
        /// </summary>
        /// <returns></returns>
        private async Task getRewardRequestList()
        {
            RequestModel request = new RequestModel();
            request.process = ProcessConstants.GET_REWARD_ALLOWANCE_REQUEST;
            request.userId = UserId;
            request.branchId = BranchId;
            request.token = Token;
            request.employeeId = EmployeeId;
            request.fromDate = FromDate;
            request.toDate = ToDate;
            request.opt = ActiveTabIndex == 0 ? "ACTIVE" : ""; // tình trạng
            request.branchIds = ListCboBranchSelected.IsNullOrEmpty() ? BranchIds : string.Join(",", ListCboBranchSelected!.Cast<ComboboxModel>().Select(m => m.id));
            request.type = CommonConstants.ENUM_LIST;
            var listResult = await _salaryService.GetMasterDataAsync<RewardAllowanceRequestModel>(request, isShowToast: true);
            listResult = listResult?.Update(m =>
            {
                Dictionary<string, string> pParams = new Dictionary<string, string>
                {
                    { "pActionType", nameof(EnumType.Update) },
                    { "pDocEntry", $"{m.id}" }
                };
                m.link = "khen-thuong-phu-cap?key=" + _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
            })?.ToList();
            if (ActiveTabIndex == 0)
            {
                ListPending = listResult;
                return;
            }
            ListAll = listResult;
        }

        private void validateData(ref string errorMessage)
        {
            if (FromDate.HasValue && ToDate.HasValue)
            {
                if (ToDate.Value.Date < FromDate.Value.Date)
                {
                    errorMessage = "Ngày đến không hợp lệ. [Từ ngày] phải nhỏ hơn [Đến ngày]";
                    return;
                }
            }
        }

        private async Task buildComboAsync()
        {
            try
            {
                var getTask4 = _masterDataService.GetBranchAsync(UserId, Token, BranchId, $"{BranchIds}", supperAdmin: IsAdmin ? "Y" : "N");
                await Task.WhenAll(
                    getTask4
                );
                ListCboBranch = (await getTask4)?.Select(m => new ComboboxModel() { id = m.branchId, code = m.branchCode, name = m.branchName })?.ToList();
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
                string errorMessage = string.Empty;
                validateData(ref errorMessage);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    return;
                }
                await ShowLoading();
                await getRewardRequestList();
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
                _navigationManager.NavigateTo($"/khen-thuong-phu-cap?key={key}");
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
                    await GridPending!.ExportToXlsxAsync("Danh-sach-khen-thuong-phu-cap", new GridXlExportOptions()
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
                await GridAll!.ExportToXlsxAsync("Danh-sach-khen-thuong-phu-cap", new GridXlExportOptions()
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
