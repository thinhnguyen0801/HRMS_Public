using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
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
    public class ContractTypeController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        const string STRING_KEY_EVENT_POST = "CONTRACT_TYPE_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "CONTRACT_TYPE_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "CONTRACT_TYPE_CONTROLLER_DELETE";
        #region Properties
        public List<ContractTypeModel>? ListContractType { get; set; }
        public IGrid? GridContractType { get; set; }
        public IReadOnlyList<object>? SelectedContractTypes { get; set; } = null;
        public ContractTypeModel ContractTypeUpdate { get; set; } = new ContractTypeModel();
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // cbo ds trạng thái nhân viên
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
                    string errMessage = await CheckMenuPermissionAsync("loai-hop-dong");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Loại hợp đồng", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await buildComboboxAsync();
                    await getContractTypes();

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
        private async Task getContractTypes()
        {
            ListContractType = new List<ContractTypeModel>();
            ListContractType = await _masterDataService.GetContractTypeAsync(UserId, Token);
        }
        
        private async Task buildComboboxAsync()
        {
            try
            {
                var getTask1 = _masterDataService.GetBranchAsync(UserId, Token);
                var getTask2 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiNhanVien)); // ds trạng thái
                await Task.WhenAll(
                    getTask1,
                    getTask2
                );
                ListCboBranch = (await getTask1)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
                ListCboStatus = (await getTask2)?.Where(m => m.rowOrder != 0).ToList();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "BuildComboAsync");
            }
        }
        
        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(ContractTypeUpdate.code))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Mã loại hợp đồng");
                fieldName = "txtCode";
                return;
            }
            if (string.IsNullOrEmpty(ContractTypeUpdate.name))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Tên loại hợp đồng");
                fieldName = "txtName";
                return;
            }
            if (ContractTypeUpdate.branchId < 1)
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chi nhánh");
                fieldName = "txtBranchId";
                return;
            }
            if (string.IsNullOrEmpty(ContractTypeUpdate.statusCode))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Trạng thái nhân viên");
                fieldName = "txtStatusCode";
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

        #region
        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getContractTypes();
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

        protected async Task OnOpenDialogHandler(EnumType pAction = EnumType.Add, ContractTypeModel? pItemDetails = null)
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
                    ContractTypeUpdate = new ContractTypeModel();
                    if (!ListCboBranch.IsNullOrEmpty()) ContractTypeUpdate.branchId = BranchId;
                }
                else
                {
                    if (!IsAllowPut)
                    {
                        ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                        return;
                    }
                    ContractTypeUpdate.id = pItemDetails!.id;
                    ContractTypeUpdate.code = pItemDetails!.code;
                    ContractTypeUpdate.name = pItemDetails!.name;
                    ContractTypeUpdate.remark = pItemDetails!.remark;
                    ContractTypeUpdate.branchId = pItemDetails!.branchId;
                    ContractTypeUpdate.statusCode = pItemDetails!.statusCode;
                    ContractTypeUpdate.duration = pItemDetails!.duration;
                    ContractTypeUpdate.isIndefiniteDuration = pItemDetails!.isIndefiniteDuration;
                    ContractTypeUpdate.numberOfDaysReduced = pItemDetails!.numberOfDaysReduced;
                    ContractTypeUpdate.isActive = pItemDetails!.isActive;
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
                string processKey = IsCreate ? ProcessConstants.POST_CONTRACTTYPE : ProcessConstants.PUT_CONTRACTTYPE;
                ContractTypeUpdate.userSign = UserId;
                ContractTypeUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(ContractTypeUpdate);
                isConfirm = await _masterDataService.UpdateContractTypeAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getContractTypes();
                    IsShowDialog = false;
                    SelectedContractTypes = null;
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
                await checkPermission(MenuId);
                if (!IsAllowDelete)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
                if (SelectedContractTypes.IsNullOrEmpty())
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
        #endregion
    }
}
