namespace HNOne.Model.Models
{
    public class ContractTypeModel
    {
        public int id { get; set; }
        public string? code { get; set; } // mã
        public string? name { get; set; }
        public string? remark { get; set; }
        public int branchId { get; set; }
        public string? statusCode { get; set; } //trạng thái nhân viên
        public int duration { get; set; } // thời hạn
        public int indefiniteDuration { get; set; } // thời hạn không xác định
        public int numberOfDaysReduced { get; set; } // số ngày cho phép giảm
        public bool isActive { get; set; }
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
