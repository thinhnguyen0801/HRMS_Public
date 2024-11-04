using DevExpress.Blazor;
using HNOne.Model.Models;

namespace HNOne.Web.Controllers
{
    public partial class EmployeeController
    {
        #region Properties
        public List<InsuranceModel>? ListInsurance { get; set; } // danh sách thông tin lương
        public IGrid? GridInsurance { get; set; }
        #endregion
    }
}
