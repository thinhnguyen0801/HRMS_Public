using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using HNOne.Web.Services;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class ContractTypeController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        #region Properties
        public List<ContractTypeModel>? ListContractType { get; set; }
        public IGrid? GridContractType { get; set; }
        public IReadOnlyList<object>? SelectedContractTypes { get; set; } = null;
        public ContractTypeModel ContractTypeUpdate { get; set; } = new ContractTypeModel();
        public EditContext? _EditContext { get; set; }
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;
        public W1Confirm confirm { get; set; }
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await ShowLoading();
                    //await _progressService.SetPercent(0.4);
                    //string errMessage = await CheckAuthMenuAsync("contractlist");
                    //if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    //Permission = await _masterDataService.GetAccessControl(UserId, Token, ContractTypeId, 10012);
                    //ItemSearch.fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    //ItemSearch.toDate = DateTime.Now;
                    //await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
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
                    //await _progressService!.Done();
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

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, ContractTypeModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    ContractTypeUpdate = new ContractTypeModel();
                }
                else
                {
                    ContractTypeUpdate.id = pItemDetails!.id;
                    ContractTypeUpdate.code = pItemDetails!.code;
                    ContractTypeUpdate.name = pItemDetails!.name;
                    ContractTypeUpdate.remark = pItemDetails!.remark;
                    ContractTypeUpdate.branchId = pItemDetails!.branchId;
                    ContractTypeUpdate.statusCode = pItemDetails!.statusCode;
                    ContractTypeUpdate.duration = pItemDetails!.duration;
                    ContractTypeUpdate.indefiniteDuration = pItemDetails!.indefiniteDuration;
                    ContractTypeUpdate.numberOfDaysReduced = pItemDetails!.numberOfDaysReduced;
                    ContractTypeUpdate.isActive = pItemDetails!.isActive;
                    IsCreate = false;
                }
                IsShowDialog = true;
                _EditContext = new EditContext(ContractTypeUpdate);
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
                var checkData = _EditContext!.Validate();
                if (!checkData) return;
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, MessageConstants.MESSAGE_CONFIRM_ADD);
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
