using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model.Models;
using HNOne.Web.Components.Controls;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace HNOne.Web.Controllers
{
    public class DocListControllerBase<TSource> : DocumentControllerBase where TSource : class
    {
        #region Properties
        public List<TSource>? ListPending { get; set; } // ds chờ xử lý
        public IGrid? GridPending { get; set; }
        public IReadOnlyList<object>? SelectedPendings { get; set; } = null;
        public List<TSource>? ListAll { get; set; } // ds tất cả - status chờ xử lý
        public IGrid? GridAll { get; set; }
        public int ActiveTabIndex { get; set; } = 0;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        #endregion Properties

        #region Public Functions  

        /// <summary>
        /// kiểm tra dữ liệu
        /// </summary>
        /// <param name="errorMessage"></param>
        public virtual void validateData(ref string errorMessage)
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
        #endregion


    }
}
