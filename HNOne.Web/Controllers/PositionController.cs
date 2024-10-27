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
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class PositionController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }

        #region Properties
        public List<PositionModel>? ListPosition { get; set; }
        public IGrid? GridPosition { get; set; }
        public IReadOnlyList<object>? SelectedPositions { get; set; } = null;
        public PositionModel PositionUpdate { get; set; } = new PositionModel();
        public EditContext? _EditContext { get; set; }
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;
        public W1Confirm confirm { get; set; }
        public List<ComboboxModel>? ListCboBranchId { get; set; } // cbo ds chi nhánh

        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    await ShowLoading();
                    await buildComboboxAsync();
                    //string errMessage = await CheckAuthMenuAsync("contractlist");
                    //if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    //Permission = await _masterDataService.GetAccessControl(UserId, Token, PositionId, 10012);
                    //ItemSearch.fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    //ItemSearch.toDate = DateTime.Now;
                    //await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await getPositions();

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
        private async Task getPositions()
        {
            ListPosition = new List<PositionModel>();
            ListPosition = await _masterDataService.GetPositionAsync(UserId, Token);
        }
        private async Task buildComboboxAsync()
        {
            try
            {
                var getTask1 = _masterDataService.GetBranchAsync(UserId, Token);
                await Task.WhenAll(
                    getTask1
                    );
                ListCboBranchId = (await getTask1)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "BuildComboAsync");
            }

        }
        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(PositionUpdate.name))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Tên chức vụ");
                fieldName = nameof(PositionUpdate.name);
                return;
            }
            if (PositionUpdate.branchId < 1)
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chi nhánh");
                fieldName = nameof(PositionUpdate.branchId);
                return;
            }
            if (string.IsNullOrEmpty(PositionUpdate.levelCode))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Cấp độ");
                fieldName = nameof(PositionUpdate.levelCode);
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
                await getPositions();
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

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, PositionModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    PositionUpdate = new PositionModel();
                }
                else
                {
                    PositionUpdate.id = pItemDetails!.id;
                    PositionUpdate.code = pItemDetails!.code;
                    PositionUpdate.name = pItemDetails!.name;
                    PositionUpdate.remark = pItemDetails!.remark;
                    PositionUpdate.isActive = pItemDetails!.isActive;
                    PositionUpdate.branchId = pItemDetails!.branchId;
                    PositionUpdate.levelCode = pItemDetails!.levelCode;
                    IsCreate = false;
                }
                IsShowDialog = true;
                _EditContext = new EditContext(PositionUpdate);
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
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, MessageConstants.MESSAGE_CONFIRM_ADD);
                if (!isConfirm) return;
                await ShowLoading();
                string processKey = IsCreate ? ProcessConstants.POST_POSITION : ProcessConstants.PUT_POSITION;
                PositionUpdate.userSign = UserId;
                PositionUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(PositionUpdate);
                isConfirm = await _masterDataService.UpdatePositionAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getPositions();
                    IsShowDialog = false;
                    SelectedPositions = null;
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
                if (SelectedPositions.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                //isConfirm = await _masterDataService.UpdatePositionAsync(processKey, UserId, Token, content);
                //if (isConfirm)
                //{
                //    await getPositions();
                //    IsShowDialog = false;
                //    SelectedPositions = null;
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
