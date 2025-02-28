

namespace HNOne.Model.Models
{
    public class WorkingBranchModel : AuditableModel
    {
        public int id { get; set; }
        public string? name { get; set; }
        public int branchId { get; set; }
        public string? branchCode { get; set; }
        public string? branchName { get; set; }
        public string? remark { get; set; }
    }
}
