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
    public class DepartmentController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        #region Properties
        public List<DepartmentModel>? ListDepartment { get; set; }
        public IGrid? GridDepartment { get; set; }
        public IReadOnlyList<object>? SelectedDepartments { get; set; } = null;
        public DepartmentModel DepartmentUpdate { get; set; } = new DepartmentModel();
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
                    await _progressService.SetPercent(0.4);
                    //string errMessage = await CheckAuthMenuAsync("contractlist");
                    //if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    //Permission = await _masterDataService.GetAccessControl(UserId, Token, DepartmentId, 10012);
                    //ItemSearch.fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    //ItemSearch.toDate = DateTime.Now;
                    //await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await getDepartments();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OnAfterRenderAsync");
                    ShowError(ex.Message);
                }
                finally
                {
                    await _progressService!.Done();
                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        #region Private Functions
        private async Task getDepartments()
        {
            ListDepartment = new List<DepartmentModel>();
            ListDepartment = await _masterDataService.GetDepartmentAsync(UserId, Token);
        }

        #endregion

        #region
        protected async Task RefreshHandler()
        {
            try
            {
                await ShowLoading();
                await getDepartments();
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

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, DepartmentModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    DepartmentUpdate = new DepartmentModel();
                }
                else
                {
                    DepartmentUpdate.id = pItemDetails!.id;
                    DepartmentUpdate.code = pItemDetails!.code;
                    DepartmentUpdate.name = pItemDetails!.name;
                    DepartmentUpdate.managerId = pItemDetails!.managerId;
                    DepartmentUpdate.headId = pItemDetails!.headId;
                    DepartmentUpdate.assistantManagerIds = pItemDetails!.assistantManagerIds;
                    DepartmentUpdate.remark = pItemDetails!.remark;
                    DepartmentUpdate.isActive = pItemDetails!.isActive;
                    DepartmentUpdate.branchId = pItemDetails!.branchId;
                    IsCreate = false;
                }
                IsShowDialog = true;
                _EditContext = new EditContext(DepartmentUpdate);
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
                string processKey = IsCreate ? ProcessConstants.POST_DEPARTMENT : ProcessConstants.PUT_DEPARTMENT;
                DepartmentUpdate.userSign = UserId;
                DepartmentUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(DepartmentUpdate);
                isConfirm = await _masterDataService.UpdateDepartmentAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getDepartments();
                    IsShowDialog = false;
                    SelectedDepartments = null;
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
                if (SelectedDepartments.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                //isConfirm = await _masterDataService.UpdateDepartmentAsync(processKey, UserId, Token, content);
                //if (isConfirm)
                //{
                //    await getDepartments();
                //    IsShowDialog = false;
                //    SelectedDepartments = null;
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
