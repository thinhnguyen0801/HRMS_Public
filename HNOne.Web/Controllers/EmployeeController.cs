using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.Web.Controllers
{
    public class EmployeeController : DocumentControllerBase
    {
        #region Properties
        public EmployeeModel EmployeeUpdate { get; set; } = new EmployeeModel();
        public List<ComboboxModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public List<ComboboxModel>? ListCboDepartment { get; set; } // cbo ds phòng ban
        public List<ComboboxModel>? ListCboManager { get; set; } // cbo ds người quản lý
        public List<ComboboxModel>? ListCboPosition { get; set; } // cbo ds người quản lý
        public List<ComboboxModel>? ListCboTitle { get; set; } // cbo ds người quản lý
        #endregion
    }
}
