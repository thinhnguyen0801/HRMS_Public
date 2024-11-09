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
        [Inject] IPersonnelService _personnelService { get; init; }

        #region Properties
        public List<ContractAppendixModel>? ListContract { get; set; }
        public IGrid? GridContract { get; set; }
        public List<ContractAppendixModel>? ListContractAll { get; set; }
        public IGrid? GridContractAll { get; set; }

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
                    string errMessage = await CheckMenuPermissionAsync("danh-sach-phu-luc-hop-dong");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Nhân sự", isActive: true),
                        new BreadcrumbModel("Danh sách phụ lục hợp đồng", isActive: true)
                    };
                    FromDate = new DateTime(DateTime.Now.Year, 01, 01);
                    ToDate = DateTime.Now;
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
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
        private async Task getContractList()
        {
            RequestModel request = new RequestModel();
            request.userId = UserId;
            request.branchId = BranchId;
            request.token = Token;
            request.opt = ActiveTabIndex == 0 ? "ACTIVE" : "";
            request.fromDate = FromDate;
            request.toDate = ToDate;
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
                _navigationManager.NavigateTo($"/chi-tiet-phu-luc-hop-dong?key={key}");
            }
            catch { }
        }
        #endregion
    }
}
