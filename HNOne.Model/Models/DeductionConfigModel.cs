namespace HNOne.Model.Models
{
    public class DeductionConfigModel : AuditableModel
    {
        public int id { get; set; }
        public int branchId { get; set; }
        public string? branchCode { get; set; }
        public string? branchName { get; set; }
        public string? type { get; set; } // loại trích nộp
        public string? typeName { get; set; } // loại trích nộp
        public double coefficientEnterprise { get; set; } // Hệ số cho doanh nghiệp
        public double coefficientEmployee { get; set; } // Hệ số cho nhân viên
        public bool isActive { get; set; }
        public DateTime? fromDate { get; set; } // Ngày bắt đầu
        public DateTime? toDate { get; set; } // Ngày kết thúc
        public decimal maxEnterprise { get; set; } // max đóng cho doanh nghiệp
        public decimal maxEmployee { get; set; } // max đóng cho nhân viên
    }
}
