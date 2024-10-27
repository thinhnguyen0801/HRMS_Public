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
        public class TitleController : DocumentControllerBase
        {
            [Inject] IMasterDataService _masterDataService { get; init; }
            [Inject] IJSRuntime _jsRuntime { get; set; }

        #region Properties
        public List<TitleModel>? ListTitle { get; set; }
            public IGrid? GridTitle { get; set; }
            public IReadOnlyList<object>? SelectedTitles { get; set; } = null;
            public TitleModel TitleUpdate { get; set; } = new TitleModel();
            public EditContext? _EditContext { get; set; }
            public bool IsShowDialog { get; set; }
            public bool IsCreate { get; set; } = true;
            public W1Confirm confirm { get; set; }
            public List<ComboboxModel>? ListCboBranchId { get; set; } // cbo ds chi nhánh
            public List<ComboboxModel>? ListCboDepartmentId { get; set; } // cbo ds phòng ban

        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
            {
                if (firstRender)
                {
                    try
                    {
                        await ShowLoading();
                        await buildComboboxAsync();
                        //await _progressService.SetPercent(0.4);
                        //string errMessage = await CheckAuthMenuAsync("contractlist");
                        //if (errMessage == "401") return; // kiểm quyền menu page danh sách
                        //Permission = await _masterDataService.GetAccessControl(UserId, Token, TitleId, 10012);
                        //ItemSearch.fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        //ItemSearch.toDate = DateTime.Now;
                        //await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                        await getTitles();

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
        private async Task getTitles()
        {
            ListTitle = new List<TitleModel>();
            ListTitle = await _masterDataService.GetTitleAsync(UserId, Token);
        }
        private async Task buildComboboxAsync()
        {
            try
            {
                var getTask1 = _masterDataService.GetBranchAsync(UserId, Token);
                var getTask2 = _masterDataService.GetDepartmentAsync(UserId, Token);
            await Task.WhenAll(
                    getTask1
                    );
                ListCboBranchId = (await getTask1)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
                ListCboDepartmentId = (await getTask2)?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
        }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "BuildComboAsync");
            }
        }
        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(TitleUpdate.name))
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Tên chức danh");
                fieldName = nameof(TitleUpdate.name);
                return;
            }
            if (TitleUpdate.branchId < 1)
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chi nhánh");
                fieldName = nameof(TitleUpdate.branchId);
                return;
            }
            if (TitleUpdate.departmentId < 1)
            {
                errorMessage = String.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Phòng ban");
                fieldName = nameof(TitleUpdate.departmentId);
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
                await getTitles();
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

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, TitleModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    TitleUpdate = new TitleModel();
                }
                else
                {
                    TitleUpdate.id = pItemDetails!.id;
                    TitleUpdate.code = pItemDetails!.code;
                    TitleUpdate.name = pItemDetails!.name;
                    TitleUpdate.remark = pItemDetails!.remark;
                    TitleUpdate.isActive = pItemDetails!.isActive;
                    TitleUpdate.branchId = pItemDetails!.branchId;
                    TitleUpdate.departmentId = pItemDetails!.departmentId;
                IsCreate = false;
                }
                IsShowDialog = true;
                _EditContext = new EditContext(TitleUpdate);
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
                string processKey = IsCreate ? ProcessConstants.POST_TITLE : ProcessConstants.PUT_TITLE;
                TitleUpdate.userSign = UserId;
                TitleUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(TitleUpdate);
                isConfirm = await _masterDataService.UpdateTitleAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getTitles();
                    IsShowDialog = false;
                    SelectedTitles = null;
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
                if (SelectedTitles.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                //isConfirm = await _masterDataService.UpdateTitleAsync(processKey, UserId, Token, content);
                //if (isConfirm)
                //{
                //    await getTitles();
                //    IsShowDialog = false;
                //    SelectedTitles = null;
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
