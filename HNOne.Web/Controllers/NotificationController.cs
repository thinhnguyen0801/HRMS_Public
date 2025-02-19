using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Models;
using HNOne.Web.Services;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace HNOne.Web.Controllers
{
    public class NotificationController : DocListControllerBase<NotificationModel>
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] DataHelperService _dataHelperService { get; set; }
        #region Properties
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    //string errMessage = await CheckMenuPermissionAsync("danh-sach-phu-luc-hop-dong");
                    //if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh sách thông báo", isActive: true)
                    };
                    FromDate = new DateTime(DateTime.Now.Year, 01, 01);
                    ToDate = DateTime.Now;
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    //initDataAsync();
                    await getNotifications();
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
        private async Task getNotifications()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.token = Token;
            request.branchId = BranchId;
            request.type = ProcessConstants.GET_COMBO_TYPE_NOTIFICATION_BY_EMPLOYEE;
            request.opt = EmployeeId.ToString();
            request.opt1 = FromDate?.ToString();
            request.opt2 = ToDate?.ToString();
            var result = await _masterDataService.GetMasterDataAsync<NotificationModel>(request);
            result = result?.Update(m =>
            {
                Dictionary<string, string> pParams = new Dictionary<string, string>
                        {
                            { "pActionType", nameof(EnumType.Update) },
                            { "pDocEntry", $"{m.docEntry}" }
                        };
                if (m.objType == nameof(EnumObjType.ContractAppendices)) pParams.TryAdd("pContractId", $"{m.contractId}");
                m.link = _dataHelperService.ListUris[$"{m.objType}"] + _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
            })?.ToList();
            if (ActiveTabIndex == 0)
            {
                ListPending = result;
                return;
            }
            ListAll = result;
        }

        #endregion

        #region
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
                await getNotifications();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "RefreshHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task SaveDataHandler()
        {
            try
            {
                if(SelectedPendings.IsNullOrEmpty())
                {
                    ShowWarning(string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Thông báo"));
                    return;
                }
                await ShowLoading();
                RequestModel request = new RequestModel();
                request.userId = UserId;
                request.token = Token;
                request.branchId = BranchId;
                request.process = ProcessConstants.POST_NOTIFICATION_STATUS_READ;
                request.opt = $"{string.Join(",", SelectedPendings!.Cast<NotificationModel>().Select(m => m.id))}";
                request.employeeId = EmployeeId;
                bool isSuccess = await _masterDataService.UpdateMasterAsync(request, isShowToast: true);
                if (isSuccess)
                {
                    await getNotifications();
                }
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "RefreshHandler");
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
