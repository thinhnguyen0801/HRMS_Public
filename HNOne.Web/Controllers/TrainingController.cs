using Microsoft.AspNetCore.Components;
using HNOne.Web.Services.Interfaces;
using HNOne.Web.Components.Controls;
using Microsoft.JSInterop;
using HNOne.Web.Commons;
using HNOne.Model.Models;
using HNOne.Model;
using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Web.Models;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using HNOne.Web.Services;

namespace HNOne.Web.Controllers
{
    public class TrainingController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IApprovalService _approvalService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        #region Properties
        public string? pActionType { get; set; } = nameof(EnumType.Add);
        private int pDocEntry { get; set; } = 0;
        public int ActiveTabIndex { get; set; } = 0;
        public TrainingModel TrainDocument { get; set; } = new TrainingModel();
        public List<LeaveRequest1Model>? ListOfTrainings { get; set; } // danh sách thông tin trong koas đạo tạo
        public IGrid? GridOfTrainings { get; set; }

        public List<EnumCatagoryModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public List<EnumCatagoryModel>? ListCboTrainFormat { get; set; } // cbo ds hình thức đào tạo
        private string? pPopupType { get; set; } = string.Empty; // mở popup nào
        public bool IsShowDialogEmpSearch { get; set; }
        public string? DepartmentIds { get; set; }
        public string? StatusIds { get; set; } // Tình trạng nào
        public object? EmployeeSelected { get; set; } // Nhân viên được chọn
        public bool firstRender = true;
        public string? VoucherHistory { get; set; } = string.Empty; // lịch sử chứng từ
        // lock control lại
        public bool IsReadonlyControl { get; set; } = false;
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    //string errMessage = await CheckMenuPermissionAsync("danh-sach-hop-dong");
                    //if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    this.firstRender = firstRender;
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Đào tạo"),
                        new BreadcrumbModel("Chứng từ đề nghị"),
                        new BreadcrumbModel("Đề nghị nghỉ phép", "danh-sach-de-nghi-nghi-phep"),
                        new BreadcrumbModel("Chi tiết đề nghị nghỉ phép", isActive: true),
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    //
                    initDataAsync();
                    await buildComboAsync();
                    if (pDocEntry > 0)
                    {
                        //await showVoucher();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OnAfterRenderAsync");
                    ShowError(ex.Message);
                }
                finally
                {
                    this.firstRender = false;
                    await ShowLoading(false);
                    await InvokeAsync(StateHasChanged);
                }
            }
        }

        #region Private Functions
        private void initDataAsync(bool isRefresh = false)
        {
            // GÁN DỮ LIỆU MẶC ĐỊNH
            var uri = _navigationManager?.ToAbsoluteUri(_navigationManager.Uri);
            if (!isRefresh && uri != null && QueryHelpers.ParseQuery(uri.Query).Count > 0)
            {
                string key = uri.Query.Substring(5); // bỏ ?key=
                Dictionary<string, string> pParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(_encryptHelper.Decrypt(key))!;
                if (pParams != null && pParams.Any())
                {
                    if (pParams.ContainsKey("pActionType")) pActionType = Convert.ToString(pParams["pActionType"]);
                    if (pParams.ContainsKey("pDocEntry")) pDocEntry = Convert.ToInt32(pParams["pDocEntry"]);
                }
            }
            IsReadonlyControl = pActionType == nameof(EnumType.Update);
        }

        private async Task buildComboAsync()
        {
            try
            {

            }
            catch (Exception) { throw; }
        }
        #endregion

        #region Protected Functions
        protected async Task OpenPopupHandler(string type = nameof(EmployeeSelected),
            string popupType = nameof(TrainDocument.employeeSignatureCode))
        {
            try
            {
                pPopupType = popupType;
                switch (type)
                {
                    case nameof(EmployeeSelected):
                        //ListCboDepartment ??= new();
                        //DepartmentIds = string.Join(",", ListCboDepartment.Select(m => m.id));
                        IsShowDialogEmpSearch = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "OpenPopupHandler");
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
