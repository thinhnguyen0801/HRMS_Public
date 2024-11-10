using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HNOne.Web.Services.Interfaces;
using DevExpress.Blazor;
using HNOne.Model.Models;
using HNOne.Web.Models;
using HNOne.Common;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class SalaryCategoryController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        const string STRING_KEY_EVENT_POST = "SALARY_CATAGORY_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "SALARY_CATAGORY_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "SALARY_CATAGORY_CONTROLLER_DELETE";
        #region Properties
        public List<SalaryCategoryModel>? ListSalary { get; set; }
        public IGrid? GridSalary { get; set; }
        public IReadOnlyList<object>? SelectedSalaries { get; set; } = null;
        public SalaryCategoryModel EntityUpdate { get; set; } = new SalaryCategoryModel();
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
                    string errMessage = await CheckMenuPermissionAsync("loai-luong");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Loại lương", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await getSalaryCatagory();

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

        private async Task getSalaryCatagory()
        {
            ListSalary = new List<SalaryCategoryModel>();
            ListSalary = await _masterDataService.GetSalaryCatagoryAsync(UserId, Token, isShowToast: true);
        }
        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(EntityUpdate.code))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Mã loại lương");
                fieldName = "txtCode";
                return;
            }
            if (string.IsNullOrEmpty(EntityUpdate.name))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Tên loại lương");
                fieldName = "txtName";
                return;
            }
        }

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
        #endregion

        #region Projected Functions
        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getSalaryCatagory();
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

        protected async Task OnOpenDialogHandler(EnumType pAction = EnumType.Add, SalaryCategoryModel? pItemDetails = null)
        {
            try
            {
                await checkPermission(MenuId);
                if (pAction == EnumType.Add)
                {
                    if (!IsAllowPost)
                    {
                        ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                        return;
                    }
                    IsCreate = true;
                    EntityUpdate = new SalaryCategoryModel();
                }
                else
                {
                    if (!IsAllowPut)
                    {
                        ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                        return;
                    }
                    EntityUpdate.id = pItemDetails!.id;
                    EntityUpdate.code = pItemDetails!.code;
                    EntityUpdate.name = pItemDetails!.name;
                    EntityUpdate.rowOrder = pItemDetails!.rowOrder;
                    EntityUpdate.isActive = pItemDetails!.isActive;
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
                string processKey = IsCreate ? ProcessConstants.POST_SALARY_CATEGORY : ProcessConstants.PUT_SALARY_CATEGORY;
                EntityUpdate.userSign = UserId;
                EntityUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(EntityUpdate);
                isConfirm = await _masterDataService.UpdateSalaryCategoryAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getSalaryCatagory();
                    IsShowDialog = false;
                    SelectedSalaries = null;
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
                if (SelectedSalaries.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                //isConfirm = await _masterDataService.UpdateBranchAsync(processKey, UserId, Token, content);
                //if (isConfirm)
                //{
                //    await getBranchs();
                //    IsShowDialog = false;
                //    SelectedBranchs = null;
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
