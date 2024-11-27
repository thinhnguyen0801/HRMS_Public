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
        public List<ComboboxModel>? ListCboEnumType { get; set; } // cbo ds loại enum
        string? enumType;
        public string? pEnumTypeId
        {
            get => enumType;
            set
            {
                if (enumType != value) _ = RefreshHandler();
                enumType = value;  
            }
        }

        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public DateTime? FromBreakTime { get; set; }
        public DateTime? ToBreakTime { get; set; }
        public double TotalHours { get; set; } = 0;
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
                        new BreadcrumbModel("Hệ thống"),
                        new BreadcrumbModel("Danh mục cấu hình", isActive: true)
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await buildComboAsync();
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

        private async Task buildComboAsync()
        {
            try
            {
                var lstResult = await _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumType)); // ds loại danh mục
                ListCboEnumType = lstResult?.Select(m => new ComboboxModel() { code = m.code, name = m.name })?.ToList();
                pEnumTypeId = ListCboEnumType?.FirstOrDefault()?.code;
            }
            catch (Exception) { throw; }
        }

        private async Task getEnumTypes()
        {
            ListEnum = new List<EnumCatagoryModel>();
            ListEnum = await _masterDataService.GetEnumAsync(UserId, Token, pEnumTypeId, isShowToast: true);
        }

        /// <summary>
        /// kiểm tra quyền nút
        /// </summary>
        /// <returns></returns>
        private async Task checkPermission(string menuId)
        {
            //List<string> lstKey = await CheckEventPermission(menuId);
            //IsAllowPost = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_POST) != null;
            //IsAllowDelete = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_DELETE) != null;
            //IsAllowPut = lstKey.FirstOrDefault(m => m == STRING_KEY_EVENT_PUT) != null;
            IsAllowPost = true;
            IsAllowDelete = true;
            IsAllowPut = true;
        }

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(EnumUpdate.code))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Mã danh mục");
                fieldName = "txtCode";
                return;
            }
            if (string.IsNullOrEmpty(EnumUpdate.name))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Tên danh mục");
                fieldName = "txtName";
                return;
            }
            if (string.IsNullOrEmpty(EnumUpdate.enumType))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, "Loại danh mục");
                fieldName = "txtenumType";
                return;
            }
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

        protected void OnOpenDialogHandler(EnumType pAction = EnumType.Add, EnumCatagoryModel? pItemDetails = null)
        {
            try
            {
                if (pAction == EnumType.Add)
                {
                    IsCreate = true;
                    EnumUpdate = new EnumCatagoryModel();
                    EnumUpdate.enumType = pEnumTypeId;
                    TotalHours = 0;
                    if (pEnumTypeId == nameof(EnumCatagory.CaLamViec))
                    {
                        FromTime = new DateTime(1900, 01, 01);
                        ToTime = new DateTime(1900, 01, 01);
                        FromBreakTime = new DateTime(1900, 01, 01);
                        ToBreakTime = new DateTime(1900, 01, 01);
                    }
                }
                else
                {
                    EnumUpdate = new EnumCatagoryModel();
                    EnumUpdate.id = pItemDetails!.id;
                    EnumUpdate.code = pItemDetails!.code;
                    EnumUpdate.name = pItemDetails!.name;
                    EnumUpdate.enumType = pItemDetails!.enumType;
                    if (EnumUpdate.enumType == nameof(EnumCatagory.CaLamViec))
                    {
                        var value = pItemDetails!.value.Split(':');
                        var value1 = pItemDetails!.value1.Split(':');
                        var value2 = pItemDetails!.value2.Split(':');
                        var value3 = pItemDetails!.value3.Split(':');
                        FromTime = new DateTime(1900, 01, 01, int.Parse(value[0]), int.Parse(value[1]), 0);
                        ToTime = new DateTime(1900, 01, 01, int.Parse(value1[0]), int.Parse(value1[1]), 0);
                        FromBreakTime = new DateTime(1900, 01, 01, int.Parse(value2[0]), int.Parse(value2[1]), 0);
                        ToBreakTime = new DateTime(1900, 01, 01, int.Parse(value3[0]), int.Parse(value3[1]), 0);
                        TotalHours = double.Parse(pItemDetails.value4 ?? "0");
                    }
                    else if (EnumUpdate.enumType == nameof(EnumCatagory.LoaiNhanVien))
                    {
                        EnumUpdate.value = pItemDetails!.value;
                    }
                    else if (EnumUpdate.enumType == nameof(EnumCatagory.CachTinhLuongPhuCap))
                    {
                        EnumUpdate.value = pItemDetails!.value;
                        TotalHours = double.Parse(pItemDetails!.value ?? "0");
                    }
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

        /// <summary>
        /// lưu thông tin
        /// </summary>
        /// <returns></returns>
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
                string processKey = IsCreate ? ProcessConstants.POST_ENUM_CATA : ProcessConstants.PUT_ENUM_CATA;
                EnumUpdate.userSign = UserId;
                EnumUpdate.userSign2 = UserId;
                EnumUpdate.enumTypeName = ListCboEnumType?.FirstOrDefault(m => m.code == EnumUpdate.enumType)?.name;
                if (EnumUpdate.enumType == nameof(EnumCatagory.CaLamViec))
                {
                    DateTime dt1 = new DateTime(1900, 01, 01, FromTime?.Hour ?? 0, FromTime?.Minute ?? 0, 0);
                    DateTime dt2 = new DateTime(1900, 01, 01, ToTime?.Hour ?? 0, ToTime?.Minute ?? 0, 0);
                    DateTime dt3 = new DateTime(1900, 01, 01, FromBreakTime?.Hour ?? 0, FromBreakTime?.Minute ?? 0, 0);
                    DateTime dt4 = new DateTime(1900, 01, 01, ToBreakTime?.Hour ?? 0, ToBreakTime?.Minute ?? 0, 0);
                    EnumUpdate.value =  dt1.ToString("yyyy-MM-dd HH:mm:ss");
                    EnumUpdate.value1 = dt2.ToString("yyyy-MM-dd HH:mm:ss");
                    EnumUpdate.value2 = dt3.ToString("yyyy-MM-dd HH:mm:ss");
                    EnumUpdate.value3 = dt4.ToString("yyyy-MM-dd HH:mm:ss");
                    TimeSpan workBeforeBreak = dt3 - dt1;
                    TimeSpan workAfterBreak = dt2 - dt4;
                    double totalWorkHours = workBeforeBreak.TotalHours + workAfterBreak.TotalHours;
                    EnumUpdate.value4 = totalWorkHours.ToString();
                }
                else if (EnumUpdate.enumType == nameof(EnumCatagory.LoaiNhanVien))
                {
                    EnumUpdate.value = EnumUpdate.value;
                }
                else if (EnumUpdate.enumType == nameof(EnumCatagory.CachTinhLuongPhuCap))
                {
                    EnumUpdate.value = TotalHours.ToString();
                }
                string content = JsonConvert.SerializeObject(EnumUpdate);
                isConfirm = await _masterDataService.UpdateContractTypeAsync(processKey, UserId, Token, content);
                if (isConfirm)
                {
                    await getEnumTypes();
                    IsShowDialog = false;
                    SelectedEnums = null;
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
