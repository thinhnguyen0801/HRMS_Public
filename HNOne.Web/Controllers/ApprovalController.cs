using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Components.Controls;
using HNOne.Web.Models;
using HNOne.Web.Services;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class ApprovalController : DocumentControllerBase
    {
        [Inject] IApprovalService _approvalService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }

        const string STRING_KEY_EVENT_APPROVAL = "APPROVAL_CONTROLLER_APPROVAL";
        const string STRING_KEY_EVENT_DENY = "APPROVAL_CONTROLLER_DENY";
        const string STRING_KEY_EVENT_ = "APPROVAL_CONTROLLER_DELETE";
        #region Properties
        public List<ApprovalModel>? ListPending { get; set; } // ds chờ xử lý
        public IGrid? GridPending { get; set; }
        public IReadOnlyList<object>? SelectedPendings { get; set; } = null;

        public List<ApprovalModel>? ListAll { get; set; } // ds tất cả - status chờ xử lý
        public IGrid? GridAll { get; set; }

        public int ActiveTabIndex { get; set; } = 0;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    //string errMessage = await CheckMenuPermissionAsync("danh-sach-phu-luc-hop-dong");
                    //if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Phê duyệt chứng từ", isActive: true)
                    };
                    FromDate = new DateTime(DateTime.Now.Year, 01, 01);
                    ToDate = DateTime.Now;
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);
                    await getApprovalList();
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
        private async Task getApprovalList()
        {
            string approvalType = ActiveTabIndex == 0 ? "O" : "";
            var lstApproval = await _approvalService.GetApprovalAsync(UserId, BranchId, EmployeeId, Token
                , approvalType, fromDate: FromDate, toDate: ToDate);
            if (ActiveTabIndex == 0)
            {
                ListPending = lstApproval;
                return;
            }
            ListAll = lstApproval;
        }

        /// <summary>
        /// kiểm tra dữ liệu
        /// </summary>
        /// <param name="errorMessage"></param>
        private void validateData(ref string errorMessage)
        {
            if (FromDate.HasValue && ToDate.HasValue)
            {
                if (ToDate.Value.Date < FromDate.Value.Date)
                {
                    errorMessage = MessageConstants.MESSAGE_FROM_DATE_TO_DATE_INVALID;
                    return;
                }
            }
        }

        private async Task saveDataApproval(string statusCode, string messageConfirm)
        {
            try
            {
                if (SelectedPendings.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NO_CHOSE_DATA);
                    return;
                }
                if(statusCode == CommonConstants.STATUS_CODE_DENY
                    || statusCode == CommonConstants.STATUS_CODE_CANCELED)
                {
                    // kiểm tra bắt nhập ghi chú phê duyệt
                    var checkItem = SelectedPendings!.Cast<ApprovalModel>().FirstOrDefault(m => string.IsNullOrWhiteSpace(m.approvalRemark));
                    if(checkItem != null)
                    {
                        ShowWarning(string.Format(MessageConstants.MESSAGE_STRING_REQUIRE, $"Ghi chú phê duyệt chứng từ số [{checkItem.voucherNo}]"));
                        return;
                    }    
                }    
                bool isConfirm = false;
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, messageConfirm);
                if (!isConfirm) return;
                await ShowLoading();
                await Task.Yield();
                var lstApproval = SelectedPendings!.Cast<ApprovalModel>().Select(m => new ApprovalModel()
                {
                    id = m.id,
                    branchId = m.branchId,
                    docEntry = m.docEntry,
                    statusCode = statusCode,
                    objType = m.objType,
                    approvalRemark = m.approvalRemark,
                    remark = m.remark,
                    employeeSignatureId = m.employeeSignatureId,
                    userSign2 = UserId
                });
                string content = JsonConvert.SerializeObject(lstApproval);
                var result = await _approvalService.UpdateApprovalAsync(ProcessConstants.PUT_APPROVAL, UserId, Token, content, approvalType: statusCode);
                if(result)
                {
                    SelectedPendings = null;
                    await getApprovalList();
                }    
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "RefreshHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion

        #region Protected Functions
        protected async Task RefreshHandler()
        {
            try
            {
                string errorMessage = string.Empty;
                validateData(ref errorMessage);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    return;
                }
                await ShowLoading();
                await getApprovalList();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "RefreshHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void GridPendingEditSavingHandler(GridEditModelSavingEventArgs e)
        {
            try
            {
                var itemEdit = (ApprovalModel)e.EditModel;
                var itemFind = ListPending?.FirstOrDefault(m => m.id == itemEdit.id);
                if (itemFind == null) return;
                itemFind.approvalRemark = itemEdit.approvalRemark;
                StateHasChanged();
            }
            catch (Exception) { }
        }

        /// <summary>
        /// phê duyệt chứng từ
        /// </summary>
        /// <returns></returns>
        protected async Task ApprovalHandler()
            => await saveDataApproval(CommonConstants.STATUS_CODE_APPROVED, MessageConstants.MESSAGE_CONFIRM_APPROVAL);

        /// <summary>
        /// từ chối chứng từ
        /// </summary>
        /// <returns></returns>
        protected async Task RejectHandler()
            => await saveDataApproval(CommonConstants.STATUS_CODE_DENY, MessageConstants.MESSAGE_CONFIRM_REJECT);
        #endregion

    }
}
