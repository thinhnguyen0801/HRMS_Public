using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Danh mục cấu trích nộp hợp đồng
    /// </summary>
    [Table("DeductionConfigs")]
    public sealed class DeductionConfigs : Auditable
    { 
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        public int BranchId { get; set; }
        [MaxLength(50)]
        [Required]
        public string? Type { get; set; } // loại trích nộp
        public double CoefficientEnterprise { get; set; } // Hệ số cho doanh nghiệp
        public double CoefficientEmployee { get; set; } // Hệ số cho nhân viên
        public bool IsActive { get; set; }
        public DateTime? FromDate { get; set; } // Ngày bắt đầu
        public DateTime? ToDate { get; set; } // Ngày kết thúc
        [Column(TypeName = "decimal(19, 6)")]
        public decimal MaxEnterprise { get; set; } // max đóng cho doanh nghiệp

        [Column(TypeName = "decimal(19, 6)")]
        public decimal MaxEmployee { get; set; } // max đóng cho nhân viên

    }
}
