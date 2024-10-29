using HNOne.Web.Components.Controls;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace HNOne.Web.Controllers
{
    public class SalaryConfigController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        #region Properties
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Loại hợp đồng", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    //await buildComboboxAsync();
                    //await getContractTypes();

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
        #endregion

        #region Protected Functions
        #endregion
    }
}
