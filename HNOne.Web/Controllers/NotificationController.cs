using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace HNOne.Web.Controllers
{
    public class NotificationController : DocListControllerBase<NotificationModel>
    {
        [Inject] IMasterDataService _masterDataService { get; init; }

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
        #endregion
    }
}
