using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using HNOne.Web.Models;
using HNOne.Web.Services;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class ReasonCategorieController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        #region Properties
        public List<ReasonCategorieModel>? ListReasonCategorie { get; set; }
        public IGrid? GridReasonCategorie { get; set; }
        public IReadOnlyList<object>? SelectedReasonCategories { get; set; } = null;
        public ReasonCategorieModel ReasonCategorieUpdate { get; set; } = new ReasonCategorieModel();
        public EditContext? _EditContext { get; set; }
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;
        public List<ComboboxModel>? ListCboType { get; set; } // cbo ds loại lý do

        
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    string errMessage = await CheckMenuPermissionAsync("ly-do");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Lý do", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await getReasonCategories();

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
        private async Task getReasonCategories()
        {
            ListReasonCategorie = new List<ReasonCategorieModel>();
            ListReasonCategorie = await _masterDataService.GetReasonCategorieAsync(UserId, Token);
        }
        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(ReasonCategorieUpdate.name))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Tên danh mục lý do");
                fieldName = nameof(ReasonCategorieUpdate.name);
                return;
            }
        }
        #endregion

        #region
        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getReasonCategories();
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

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, ReasonCategorieModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    ReasonCategorieUpdate = new ReasonCategorieModel();
                }
                else
                {
                    ReasonCategorieUpdate.id = pItemDetails!.id;
                    ReasonCategorieUpdate.name = pItemDetails!.name;
                    ReasonCategorieUpdate.type = pItemDetails!.type;
                    ReasonCategorieUpdate.isActive = pItemDetails!.isActive;
                    IsCreate = false;
                }
                IsShowDialog = true;
                _EditContext = new EditContext(ReasonCategorieUpdate);
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
                string errorMessage = string.Empty;
                string fieldName = string.Empty;
                validateForSave(ref errorMessage, ref fieldName);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                    return;
                }
                errorMessage = IsCreate ? MessageConstants.MESSAGE_CONFIRM_ADD : MessageConstants.MESSAGE_CONFIRM_UPDATE;
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = IsCreate ? ProcessConstants.POST_REASONCATEGORIE : ProcessConstants.PUT_REASONCATEGORIE;
                ReasonCategorieUpdate.userSign = UserId;
                ReasonCategorieUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(ReasonCategorieUpdate);
                isConfirm = await _masterDataService.UpdateReasonCategorieAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getReasonCategories();
                    IsShowDialog = false;
                    SelectedReasonCategories = null;
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

        public async Task DeleteDataHandler()
        {
            try
            {
                if (SelectedReasonCategories.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                //isConfirm = await _masterDataService.UpdateReasonCategorieAsync(processKey, UserId, Token, content);
                //if (isConfirm)
                //{
                //    await getReasonCategories();
                //    IsShowDialog = false;
                //    SelectedReasonCategories = null;
                //}
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
        #endregion
    }
}
