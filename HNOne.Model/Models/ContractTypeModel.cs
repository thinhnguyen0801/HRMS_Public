namespace HNOne.Model.Models
{
    public class ContractTypeModel : AuditableModel
    {
        public int id { get; set; }
        public string? code { get; set; } // mã
        public string? name { get; set; }
        public string? remark { get; set; }
        public int branchId { get; set; }
        public string? branchCode { get; set; }
        public string? branchName { get; set; }
        public string? statusCode { get; set; } //trạng thái nhân viên
        public string? statusName { get; set; } //trạng thái nhân viên
        public double duration { get; set; } // thời hạn
        public bool isIndefiniteDuration { get; set; } // không xác định thời hạn
        public int numberOfDaysReduced { get; set; } // số ngày cho phép giảm
        public bool isActive { get; set; }
    }
}
