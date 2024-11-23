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
    public class ShiftChangeRequestListController : DocumentControllerBase
    {
        [Inject] IWorkforceService _workforceService { get; init; }
        #region Properties
        public List<ShiftChangeModel>? ListPending { get; set; }
        public IGrid? GridPending { get; set; }
        public List<ShiftChangeModel>? ListAll { get; set; }
        public IGrid? GridAll { get; set; }

        public int ActiveTabIndex { get; set; } = 0;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

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
                        new BreadcrumbModel("Công - Phép", isActive: true),
                        new BreadcrumbModel("Danh sách đăng ký đổi ca", isActive: true)
                    };
                    FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 01);
                    ToDate = DateTime.Now;
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await getShiftRequestList();
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
        private async Task getShiftRequestList()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.branchId = BranchId;
            request.token = Token;
            request.opt = ActiveTabIndex == 0 ? "ACTIVE" : ""; // tình trạng
            request.fromDate = FromDate;
            request.toDate = ToDate;
            request.process = ProcessConstants.GET_SHIFT_CHANGE_REQUEST;
            var listResult = await _workforceService.GetShiftChangeRequestAsync(request, isShowToast: true);
            listResult = listResult?.Update(m =>
            {
                Dictionary<string, string> pParams = new Dictionary<string, string>
                {
                    { "pActionType", nameof(EnumType.Update) },
                    { "pDocEntry", $"{m.id}" }
                };
                m.link = "dang-ky-doi-ca?key=" + _encryptHelper.Encrypt(JsonConvert.SerializeObject(pParams));
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
                await getShiftRequestList();
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
                _navigationManager.NavigateTo($"/dang-ky-doi-ca?key={key}");
            }
            catch { }
        }
        #endregion
    }
}
