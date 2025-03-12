
namespace HNOne.Model.Models
{
    public class ReasonCategorieModel
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? type { get; set; } // loại lý do
        public string? typeName { get; set; } // loại lý do
        public bool isActive { get; set; } = true;
        public string? value { get; set; } // config nếu có
        public string? value1 { get; set; } // config nếu có 1
        public string? value2 { get; set; } // config nếu có 2
        public decimal config { get; set; } // config nếu có
        public decimal config1 { get; set; } // config nếu có 1
        public decimal config2 { get; set; } // config nếu có 2
        public int branchId { get; set; }
        public string? branchCode { get; set; }
        public string? branchName { get; set; }
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
