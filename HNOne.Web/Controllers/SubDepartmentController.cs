using DevExpress.Blazor;
using DevExpress.Blazor.Primitives.Internal;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class SubDepartmentController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        const string STRING_KEY_EVENT_POST = "SUB_DEPARTMENT_CONTROLLER_POST";
        const string STRING_KEY_EVENT_PUT = "SUB_DEPARTMENT_CONTROLLER_PUT";
        const string STRING_KEY_EVENT_DELETE = "SUB_DEPARTMENT_CONTROLLER_DELETE";
        #region Properties
        public List<TitleModel>? ListTitle { get; set; }
        public IGrid? GridTitle { get; set; }
        public IReadOnlyList<object>? SelectedTitles { get; set; } = null;
        public TitleModel TitleUpdate { get; set; } = new TitleModel();
        public bool IsShowDialog { get; set; }
        public bool IsCreate { get; set; } = true;
        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban

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
                    string errMessage = await CheckMenuPermissionAsync("bo-phan");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    await checkPermission(errMessage);
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Danh mục"),
                        new BreadcrumbModel("Bộ phận", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await buildComboboxAsync();
                    await getSubDepartments();

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
        private async Task getSubDepartments()
        {
            ListTitle = new List<TitleModel>();
            ListTitle = await _masterDataService.GetSubDepartmentAsync(UserId, Token, BranchId, $"{BranchIds}", isShowToast: true);
        }
        private async Task buildComboboxAsync()
        {
            try
            {
                var getTask1 = _masterDataService.GetBranchAsync(UserId, Token, BranchId, $"{BranchIds}", supperAdmin: IsAdmin ? "Y" : "N");
                await Task.WhenAll(getTask1);
                ListCboBranch = (await getTask1)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
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
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Tên bộ phận");
                fieldName = "txtName";
                return;
            }
            if (TitleUpdate.branchId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Chi nhánh");
                fieldName = "txtBranchId";
                return;
            }
            if (TitleUpdate.departmentId < 1)
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Phòng ban");
                fieldName = "txtDepartmentId";
                return;
            }
        }

        /// <summary>
        /// lấy phòng ban theo chi nhánh
        /// </summary>
        /// <param name="branchId"></param>
        /// <returns></returns>
        private async Task getDepartmentByBranchId(int branchId)
        {
            ListCboDepartment = new List<ComboboxModel>();
            var lstResult = await _masterDataService.GetDepartmentAsync(UserId, Token, branchId, opt: CommonConstants.ENUM_ACTIVE);
            ListCboDepartment = lstResult?.Select(m => new ComboboxModel() { id = m.id, name = m.name })?.ToList();
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
                await getSubDepartments();
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

        protected async Task OnOpenDialogHandler(EnumType pAction = EnumType.Add, TitleModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    TitleUpdate = new TitleModel();
                    if (!ListCboBranch.IsNullOrEmpty())
                    {
                        TitleUpdate.branchId = BranchId;
                        await ComboboxValueChangedHandler(TitleUpdate.branchId, controlID: nameof(TitleUpdate.branchId));
                    }
                }
                else
                {
                    await ComboboxValueChangedHandler(pItemDetails!.branchId, controlID: nameof(TitleUpdate.branchId));
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
                await checkPermission(MenuId);
                if ((IsCreate && !IsAllowPost) || (!IsCreate && !IsAllowPut))
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
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
                string processKey = IsCreate ? ProcessConstants.POST_SUB_DEPARTMENT : ProcessConstants.PUT_SUB_DEPARTMENT;
                TitleUpdate.userSign = UserId;
                TitleUpdate.userSign2 = UserId;
                string content = JsonConvert.SerializeObject(TitleUpdate);
                isConfirm = await _masterDataService.UpdateSubDepartmentAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getSubDepartments();
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
                await checkPermission(MenuId);
                if (!IsAllowDelete)
                {
                    ShowInfo(MessageConstants.MESSAGE_NO_PERMISSION);
                    return;
                }
                if (SelectedTitles.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, $"{MessageConstants.MESSAGE_CONFIRM_DELETE} ");
                if (!isConfirm) return;
                await ShowLoading();
                string tableName = _encryptHelper.Encrypt(nameof(EnumObjType.SubDepartments));
                string pKey = _encryptHelper.Encrypt(nameof(TitleModel.id));
                string fKey = _encryptHelper.Encrypt(nameof(EmployeeModel.subDepartmentId));
                string ids = string.Join(",", SelectedTitles!.Cast<TitleModel>().Select(m => m.id));
                string reasonDelete = "";
                string strResult = await _masterDataService.DeleteDynnamicAsync(UserId, Token, BranchId, tableName, pKey, fKey, ids, reasonDelete);
                if (strResult == "-1") return;
                if (strResult == StatusCodes.Status200OK.ToString())
                {
                    await getSubDepartments();
                    SelectedTitles = null;
                    return;
                }
                await Task.Delay(75);
                await ShowLoading(false);
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_NOTIFICATION, $"{strResult} ", isShowFooter: false);
                return;
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

        /// <summary>
        /// thay đổi combobox
        /// </summary>
        /// <param name="value"></param>
        /// <param name="controlID"></param>
        /// <returns></returns>
        protected async Task ComboboxValueChangedHandler(object? value, string controlID = nameof(TitleUpdate.branchId))
        {
            try
            {
                switch (controlID)
                {
                    case nameof(TitleUpdate.branchId):
                        int.TryParse(value?.ToString(), out int oBranchId);
                        TitleUpdate.branchId = oBranchId;
                        TitleUpdate.departmentId = 0;
                        TitleUpdate.departmentCode = "";
                        TitleUpdate.departmentName = "";
                        await ShowLoading();
                        await getDepartmentByBranchId(oBranchId);
                        await Task.Delay(100);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "ComboboxValueChangedHandler");
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// Kết xuất dữ liệu sang file excel
        /// xlsx
        /// </summary>
        /// <returns></returns>
        protected async Task ExportExcelHandler()
        {
            try
            {
                if (GridTitle == null || ListTitle.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                await ShowLoading();
                await GridTitle!.ExportToXlsxAsync("Bo-phan", new GridXlExportOptions()
                {
                    ExportTotalSummaries = false,
                    ExportGroupSummaries = false
                });
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ExportExcelHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion
    }
}
