using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class LeaveConfigController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IWorkforceService _workforceService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        const string STRING_KEY_EVENT_POST = "LEAVE_CONFIG_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "LEAVE_CONFIG_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "LEAVE_CONFIG_CONTROLLER_DELETE";

        #region Properties
        public List<LeaveConfigModel>? ListLeaveConfig { get; set; }
        public IGrid? GridLeaveConfig { get; set; }
        public IReadOnlyList<object>? SelectedItems{ get; set; } = null;
        public LeaveConfigModel LeaveConfigUpdate { get; set; } = new LeaveConfigModel();
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;

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
                    string errMessage = await CheckMenuPermissionAsync("cau-hinh-thong-so-phep");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Thông số phép năm", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await getLeaveConfig();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OnAfterRenderAsync");
                    ShowError(ex.Message);
                }
                finally
                {
                    await ShowLoading(false);
                    //await _progressService!.Done();
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
        
        private async Task getLeaveConfig()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.branchId = BranchId;
            request.process = ProcessConstants.GET_LEAVE_CONFIG;
            ListLeaveConfig = new List<LeaveConfigModel>();
            ListLeaveConfig = await _workforceService.GetMasterDataAsync<LeaveConfigModel>(request, isShowToast: true);
        }
        #endregion

        #region Protected Functions
        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getLeaveConfig();
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

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, LeaveConfigModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    LeaveConfigUpdate = new LeaveConfigModel();
                    LeaveConfigUpdate.year = DateTime.Now.Year;
                    LeaveConfigUpdate.fromDate = DateTime.Now;
                    LeaveConfigUpdate.toDate = DateTime.Now;
                    LeaveConfigUpdate.isActive = true;
                    LeaveConfigUpdate.accrualDate = 1;
                    LeaveConfigUpdate.numOfLeave = 12;
                }
                else
                {
                    LeaveConfigUpdate.id = pItemDetails!.id;
                    LeaveConfigUpdate.year = pItemDetails!.year;
                    LeaveConfigUpdate.fromDate = pItemDetails!.fromDate;
                    LeaveConfigUpdate.toDate = pItemDetails!.toDate;
                    LeaveConfigUpdate.expiryDate = pItemDetails!.expiryDate;
                    LeaveConfigUpdate.numOfYearIncrease = pItemDetails!.numOfYearIncrease;
                    LeaveConfigUpdate.numOfLeaveIncrease = pItemDetails!.numOfLeaveIncrease;
                    LeaveConfigUpdate.numOfLeaveTransfer = pItemDetails!.numOfLeaveTransfer;
                    LeaveConfigUpdate.isOffSaturday = pItemDetails!.isOffSaturday;
                    LeaveConfigUpdate.isOffSunday = pItemDetails!.isOffSunday;
                    LeaveConfigUpdate.isActive = pItemDetails!.isActive;
                    LeaveConfigUpdate.accrualDate = pItemDetails!.accrualDate;
                    LeaveConfigUpdate.numOfLeave = pItemDetails!.numOfLeave;
                    IsCreate = false;
                }
                IsShowDialog = true;
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "OnOpenDialogHandler");
                ShowError(ex.Message);
            }
        }

        protected async Task SaveDataHandler()
        {
            try
            {
                await checkPermission(MenuId);
                if ((IsCreate && !IsAllowPost) || (!IsCreate && !IsAllowPut))
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
                string errorMessage = string.Empty;
                string fieldName = string.Empty;
                //validateForSave(ref errorMessage, ref fieldName);
                //if (!string.IsNullOrEmpty(errorMessage))
                //{
                //    ShowWarning(errorMessage);
                //    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                //    return;
                //}
                errorMessage = IsCreate ? MessageConstants.MESSAGE_CONFIRM_ADD : MessageConstants.MESSAGE_CONFIRM_UPDATE;
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                LeaveConfigUpdate.userSign = UserId;
                LeaveConfigUpdate.userSign2 = UserId;
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.token = Token;
                request.branchId = BranchId;
                request.process = IsCreate ? ProcessConstants.POST_LEAVE_CONFIG : ProcessConstants.PUT_LEAVE_CONFIG;
                request.json = JsonConvert.SerializeObject(LeaveConfigUpdate);
                isConfirm = await _workforceService.UpdateMasterDataAsync(request);
                if (isConfirm)
                {
                    await getLeaveConfig();
                    IsShowDialog = false;
                    SelectedItems = null;
                }
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "SaveDataHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(50);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// xóa danh mục loại hợp đồng
        /// </summary>
        /// <returns></returns>
        public async Task DeleteDataHandler()
        {
            try
            {
                await checkPermission(MenuId);
                if (!IsAllowDelete)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
                if (SelectedItems.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                await ShowLoading();
                string tableName = _encryptHelper.Encrypt(nameof(EnumObjType.LeaveConfigs));
                string pKey = _encryptHelper.Encrypt(nameof(LeaveConfigModel.id));
                string fKey = _encryptHelper.Encrypt("LeaveConfigId");
                string ids = string.Join(",", SelectedItems!.Cast<LeaveConfigModel>().Select(m => m.id));
                string reasonDelete = "";
                string strResult = await _masterDataService.DeleteDynnamicAsync(UserId, Token, BranchId, tableName, pKey, fKey, ids, reasonDelete);
                if (strResult == "-1") return;
                if (strResult == StatusCodes.Status200OK.ToString())
                {
                    await getLeaveConfig();
                    SelectedItems = null;
                    return;
                }
                await Task.Delay(75);
                await ShowLoading(false);
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_NOTIFICATION, $"{strResult} ", isShowFooter: false);
                return;
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "DeleteDataHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(50);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
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
                if (GridLeaveConfig == null || ListLeaveConfig.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                await ShowLoading();
                await GridLeaveConfig!.ExportToXlsxAsync("Cau-hinh-thong-so-phep", new GridXlExportOptions()
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
        #endregion
    }
}
