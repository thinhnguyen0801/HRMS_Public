using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Tính lương -> khóa kỳ tính thuế thu nhập cá nhân
    /// Khi có dữ liệu thay đổi thì cũng không ảnh hưởng đến các kết quả tháng trước
    /// </summary>
    [Table("PIncomeTaxPeriods")]
    public sealed class PIncomeTaxPeriods
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Không tự tăng
        public int Id { get; set; }
        public int EmployeeId { get; set; } // id nhân viên
        [MaxLength(50)]
        [Required]
        public string? EmployeeCode { get; set; } // Mã nhân viên
        public int BranchId { get; set; } // id chi nhánh
        public int Month { get; set; } // tháng công
        public int Year { get; set; } // năm công
        public bool IsLocked { get; set; } // chốt chưa, chốt những ai
        public int AttendanceSummaryId { get; set; } // Id bảng công
        public int TaxtRateId { get; set; } // Id mức thuế
        public int TaxBracket { get; set; } // bậc thuế

        [Column(TypeName = "decimal(19, 6)")]
        public decimal MinTaxSalary { get; set; } // giời hạn tính thuế

        [Column(TypeName = "decimal(19, 6)")]
        public decimal MaxTaxSalary { get; set; } // giời hạn tính thuế
        public double TaxRate { get; set; } // % thuế

        [Column(TypeName = "decimal(19, 6)")]
        public decimal ProgressiveAmount { get; set; } // số tiền lũy tiến

        [Column(TypeName = "decimal(19, 6)")]
        public decimal StandardTax { get; set; } // Thuế giảm trừ bản thân

        [Column(TypeName = "decimal(19, 6)")]
        public decimal FamilyCircumstanceTaxDeduction { get; set; } // Giảm trừ gia cảnh

        public int NumOfPeopleFCTaxDeduction { get; set; } // số người giảm trừ

        [Column(TypeName = "decimal(19, 6)")]
        public decimal TotalFCTaxDeduction { get; set; } // tổng tiền giảm trừ gia cảnh

        [Column(TypeName = "decimal(19, 6)")]
        public decimal TaxableIncome { get; set; } // Thu nhập tính thuế

        [Column(TypeName = "decimal(19, 6)")]
        public decimal TaxAllowance { get; set; } // phụ cấp tính thuế

        [Column(TypeName = "decimal(19, 6)")]
        public decimal TaxPayment { get; set; } // Số tiền đóng thuế

    }
}
