namespace HNOne.Model.Models
{
    public class InsuranceModel : AuditableModel
    {
        public int id { get; set; }
        public int employeeId { get; set; } // mã nhân viên
        public string? insuranceType { get; set; } // Loại bảo hiểm khai ở enum
        public string? insuranceTypeName { get; set; } // Loại bảo hiểm khai ở enum
        public string? insuranceNo { get; set; } // Số bảo hiểm
        public DateTime? startDate { get; set; } // Ngày cấp
        public DateTime? endDate { get; set; } // Ngày hết hạn
        public decimal rate { get; set; } // tỉ lệ đóng BH
        public string? zipCode { get; set; } // mã tỉnh cấp
        public string? address { get; set; } // Nơi đăng kí khám chửa bệnh
        public string? addressNo { get; set; } // mã nơi đăng kí
    }
}
