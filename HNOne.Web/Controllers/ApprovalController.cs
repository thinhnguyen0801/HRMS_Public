using DevExpress.Blazor;
using HNOne.Model.Models;
using HNOne.Web.Components.Controls;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace HNOne.Web.Controllers
{
    public class ApprovalController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
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


    }
}
