

namespace HNOne.Model.Models
{
    public class DepartmentModel
    {
        public int id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public int managerId { get; set; } // Id giám đốc
        public int headId { get; set; } // Id trưởng phòng
        public string? assistantManagerIds { get; set; } // có thể có nhiều phó phòng
        public string? remark { get; set; }
        public bool isActive { get; set; } = true;
        public int branchId { get; set; }
        public DateTime? createDate { get; set; }
        public int? userSign { get; set; }
        public DateTime? updateDate { get; set; }
        public int? userSign2 { get; set; }
        public bool isDelete { get; set; }
        public string? deleteReason { get; set; }
        public DateTime? dateTracking { get; set; }
        public string? userSignName { get; set; }
        public string? userSign2Name { get; set; }
    }
}
