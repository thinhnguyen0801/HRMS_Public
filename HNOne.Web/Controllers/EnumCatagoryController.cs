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
    public class EnumCatagoryController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        const string STRING_KEY_EVENT_POST = "ENUM_CATAGORY_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "ENUM_CATAGORY_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "ENUM_CATAGORY_CONTROLLER_DELETE";

        #region Properties
        public List<EnumCatagoryModel>? ListEnum { get; set; }
        public IGrid? GridEnum { get; set; }
        public IReadOnlyList<object>? SelectedEnums { get; set; } = null;
        public EnumCatagoryModel EnumUpdate { get; set; } = new EnumCatagoryModel();
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;
        public List<ComboboxModel>? ListCboStatus { get; set; } // cbo ds loại enum

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
                    string errMessage = await CheckMenuPermissionAsync("danh-muc-cau-hinh-chung");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Danh mục cấu hình", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    //await buildComboboxAsync();
                    await getEnumTypes();

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
        private async Task getEnumTypes()
        {
            ListEnum = new List<EnumCatagoryModel>();
            ListEnum = await _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumType.AllowEdit), isShowToast: true);
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

        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getEnumTypes();
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

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, ContractTypeModel? pItemDetails = null)
        {
            try
            {
                //if (pAction == EnumType.Add)
                //{
                //    IsCreate = true;
                //    ContractTypeUpdate = new ContractTypeModel();
                //    if (!ListCboBranch.IsNullOrEmpty()) ContractTypeUpdate.branchId = BranchId;
                //}
                //else
                //{
                //    ContractTypeUpdate.id = pItemDetails!.id;
                //    ContractTypeUpdate.code = pItemDetails!.code;
                //    ContractTypeUpdate.name = pItemDetails!.name;
                //    ContractTypeUpdate.remark = pItemDetails!.remark;
                //    ContractTypeUpdate.branchId = pItemDetails!.branchId;
                //    ContractTypeUpdate.statusCode = pItemDetails!.statusCode;
                //    ContractTypeUpdate.duration = pItemDetails!.duration;
                //    ContractTypeUpdate.isIndefiniteDuration = pItemDetails!.isIndefiniteDuration;
                //    ContractTypeUpdate.numberOfDaysReduced = pItemDetails!.numberOfDaysReduced;
                //    ContractTypeUpdate.isActive = pItemDetails!.isActive;
                //    IsCreate = false;
                //}
                //IsShowDialog = true;
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
                //string errorMessage = string.Empty;
                //string fieldName = string.Empty;
                //validateForSave(ref errorMessage, ref fieldName);
                //if (!string.IsNullOrEmpty(errorMessage))
                //{
                //    ShowWarning(errorMessage);
                //    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                //    return;
                //}
                //errorMessage = IsCreate ? MessageConstants.MESSAGE_CONFIRM_ADD : MessageConstants.MESSAGE_CONFIRM_UPDATE;
                //bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                //if (!isConfirm) return;
                //await ShowLoading();
                //string processKey = IsCreate ? ProcessConstants.POST_CONTRACTTYPE : ProcessConstants.PUT_CONTRACTTYPE;
                //ContractTypeUpdate.userSign = UserId;
                //ContractTypeUpdate.userSign2 = UserId;
                //string content = JsonConvert.SerializeObject(ContractTypeUpdate);
                //isConfirm = await _masterDataService.UpdateContractTypeAsync(processKey, UserId, Token, content);
                //if (isConfirm)
                //{
                //    await getContractTypes();
                //    IsShowDialog = false;
                //    SelectedContractTypes = null;
                //}
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
                if (SelectedEnums.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                //isConfirm = await _masterDataService.UpdateContractTypeAsync(processKey, UserId, Token, content);
                //if (isConfirm)
                //{
                //    await getContractTypes();
                //    IsShowDialog = false;
                //    SelectedContractTypes = null;
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
    }
}
