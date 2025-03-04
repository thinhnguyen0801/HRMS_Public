using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class ContractAppendixListController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IPersonnelService _personnelService { get; init; }
        const string STRING_KEY_EVENT_POST = "CONTRACT_APPENDIX_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "CONTRACT_APPENDIX_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "CONTRACT_APPENDIX_CONTROLLER_DELETE";
        #region Properties
        public List<ContractAppendixModel>? ListContract { get; set; }
        public IGrid? GridContract { get; set; }
        public List<ContractAppendixModel>? ListContractAll { get; set; }
        public IGrid? GridContractAll { get; set; }
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public IReadOnlyList<object>? ListCboBranchSelected { get; set; } // chi nhánh được chọn
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public IReadOnlyList<object>? ListCboDepartmentSelected { get; set; }

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
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-phu-luc-hop-dong");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Nhân sự", isActive: true),
                        new BreadcrumbModel("Danh sách phụ lục hợp đồng", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    FromDate = new DateTime(DateTime.Now.Year, 01, 01);
                    ToDate = DateTime.Now;
                    await buildComboAsync();
                    await getContractList();
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
            List<string> lstKey = await CheckEventPermission(menuId);
            IsAllowPost = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_POST) != null;
            IsAllowDelete = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_DELETE) != null;
            IsAllowPut = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_PUT) != null;
        }

        private async Task getContractList()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.branchId = BranchId;
            request.token = Token;
            request.opt = ActiveTabIndex == 0 ? "ACTIVE" : "";
            request.fromDate = FromDate;
            request.toDate = ToDate;
            request.employeeId = EmployeeId;
            request.branchIds = ListCboBranchSelected.IsNullOrEmpty() ? BranchIds : string.Join(",", ListCboBranchSelected!.Cast<ComboboxModel>().Select(m => m.id));
            request.departmentIds = ListCboDepartmentSelected.IsNullOrEmpty() ? DepartmentIds : string.Join(",", ListCboDepartmentSelected!.Cast<ComboboxModel>().Select(m => m.id));
            request.type = CommonConstants.ENUM_LIST;
            var lstContract = await _personnelService.GetContractAppendixAsync(request, isShowToast: true);
            lstContract = lstContract?.Update(m =>
            {
                Dictionary<string, string> pParams = new Dictionary<string, string>
                {
                    { "pActionType", nameof(EnumType.Update) },
                    { "pDocEntry", $"{m.id}" },
                    { "pContractId", $"{m.contractId}" },
                };
                m.link = _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
            })?.ToList();
            if (ActiveTabIndex == 0)
            {
                ListContract = lstContract;
                return;
            }
            ListContractAll = lstContract;
        }

        /// <summary>
        /// kiểm tra dữ liệu
        /// </summary>
        /// <param name="errorMessage"></param>
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
                var getTask1 = _masterDataService.GetDepartmentAsync(UserId, Token, BranchId, $"{BranchIds}", departmentIds: $"{DepartmentIds}", opt: CommonConstants.ENUM_FILTER); // ds phòng ban
                var getTask4 = _masterDataService.GetBranchAsync(UserId, Token, BranchId, $"{BranchIds}", supperAdmin: IsAdmin ? "Y" : "N");
                await Task.WhenAll(
                    getTask1,
                    getTask4
                );
                ListCboDepartment = (await getTask1)?.Select(m => new ComboboxModel() { id = m.id, code = m.code, name = m.name })?.ToList();
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
                await getContractList();
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
                _navigationManager.NavigateTo($"/chi-tiet-phu-luc-hop-dong?key={key}");
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
                if(ActiveTabIndex == 0)
                {
                    if (GridContract == null || ListContract.IsNullOrEmpty())
                    {
                        ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                        return;
                    }
                    await ShowLoading();
                    await GridContract!.ExportToXlsxAsync("Danh-sach-phu-luc-hop-dong", new GridXlExportOptions()
                    {
                        ExportTotalSummaries = false,
                        ExportGroupSummaries = false
                    });
                    return;
                }
                if (GridContractAll == null || ListContractAll.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                await ShowLoading();
                await GridContractAll!.ExportToXlsxAsync("Danh-sach-phu-luc-hop-dong", new GridXlExportOptions()
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
