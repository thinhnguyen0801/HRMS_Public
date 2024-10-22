using DevExpress.Blazor;
using HNOne.Model.Entities;

namespace HNOne.Web.Controllers
{
    public class BranchController : DocumentControllerBase
    {
        #region Properties
        public List<Branchs>? ListBranch { get; set; }
        public IGrid? GridBranch { get; set; }
        #endregion
    }
}
