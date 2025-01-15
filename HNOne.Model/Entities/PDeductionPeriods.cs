using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Bảng lưu thông tin về trích nộp của công ty để khóa kỳ lương
    /// </summary>
    [Table("PDeductionPeriods")]
    public sealed class PDeductionPeriods
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Không tự tăng
        public long Id { get; set; }
        public int EmployeeId { get; set; } // id nhân viên
        [MaxLength(50)]
        [Required]
        public string? EmployeeCode { get; set; } // Mã nhân viên
        public int BranchId { get; set; } // id chi nhánh
        public int Month { get; set; } // tháng công
        public int Year { get; set; } // năm công
        public bool IsLocked { get; set; } // chốt chưa, chốt những ai
        public int AttendanceSummaryId { get; set; } // Id bảng công
        public bool IsCompanyDeduction { get; set; } // công ty đóng trích nộp thay
        public int DeductionConfigId { get; set; }
        [MaxLength(50)]
        [Required]
        public string? Type { get; set; } // loại

        [Column(TypeName = "decimal(19, 6)")]
        public decimal ContributionSalary { get; set; }
        public float CoefficientEnterprise { get; set; }
        public float CoefficientEmployee { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal DeductionEnterprise { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal DeductionEmployee { get; set; }
        public DateTime? FromDate { get; set; } // Ngày bắt đầu
        public DateTime? ToDate { get; set; } // Ngày kết thúc

        [Column(TypeName = "decimal(19, 6)")]
        public decimal MaxEnterprise { get; set; } // max đóng cho doanh nghiệp

        [Column(TypeName = "decimal(19, 6)")]
        public decimal MaxEmployee { get; set; } // max đóng cho nhân viên
    }
}
