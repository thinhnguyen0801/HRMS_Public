using DevExpress.Blazor;
using HNOne.Model.Models;

namespace HNOne.Web.Controllers
{
    public class EmployeeListController : DocumentControllerBase
    {
        #region Properties
        public List<EmployeeModel>? ListEmployee { get; set; }
        public IGrid? GridEmployee { get; set; }
        #endregion
    }
}
