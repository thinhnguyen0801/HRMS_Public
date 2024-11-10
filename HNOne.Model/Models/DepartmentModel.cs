

namespace HNOne.Model.Models
{
    public class DepartmentModel : AuditableModel
    {
        public int id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public int managerId { get; set; } // Id giám đốc
        public int headId { get; set; } // Id trưởng phòng
        public string? headCode { get; set; } //
        public string? headName { get; set; } // 
        public string? assistantManagerIds { get; set; } // có thể có nhiều phó phòng
        public string? assistantManagerCode { get; set; } //
        public string? assistantManagerName { get; set; } // 
        public string? remark { get; set; }
        public bool isActive { get; set; } = true;
        public int branchId { get; set; }
        public string? branchCode { get; set; }
        public string? branchName { get; set; }
    }
}
