using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HNOne.Model.Entities
{
    /// <summary>
    /// Khen thưởng & phụ cấp cho nhân viên
    /// </summary>
    [Table("RewardAllowanceRequests")]
    public sealed class RewardAllowanceRequests : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string? VoucherNo { get; set; } // số chứng từ
        public int SalaryConfigId { get; set; } // loại cấu hình lương
        [MaxLength(250)]
        [Required]
        public string? RewardName { get; set; } // Tên đợt
        public DateTime RewardDate { get; set; } // tháng thưởng
        public DateTime RewardPaymentDate { get; set; } // tháng chi thưởng
        public int EmployeeSignatureId { get; set; } // nhân viên kí
        public DateTime? DateOfSigning { get; set; } // ngày kí
        public int BranchId { get; set; }
        [MaxLength(50)]
        [Required]
        public string? StatusCode { get; set; }
        [MaxLength(250)]
        public string? NoteForAll { get; set; } // Nội dung

        [Column(TypeName = "decimal(19, 6)")]
        public decimal TotalReward { get; set; } // Tổng tiền thưởng
    }

    /// <summary>
    /// Khen thưởng & phụ cấp cho nhân viên chi tiết
    /// </summary>
    [Table("RewardAllowanceRequest1s")]
    public sealed class RewardAllowanceRequest1s
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public int Id { get; set; }
        public int RewardAllowanceId { get; set; } // id thông tin điều chỉnh phép
        public int EmployeeId { get; set; } // nhân viên
        public string? SalaryCalculateMethod { get; set; } // cách tính lương lấy ở enum
        public string? SalaryCalculateMethodName { get; set; } // cách tính lương lấy ở enum
        [Column(TypeName = "decimal(19, 6)")]
        public decimal RewardAmount { get; set; } // tiền thưởng
        [Column(TypeName = "decimal(19, 6)")]
        public decimal TaxPayment { get; set; } // tiền thuế phải đóng
        [Column(TypeName = "decimal(19, 6)")]
        public decimal PaidAmount { get; set; } //  số tiền chi trả cho nhân viên
        [Column(TypeName = "decimal(19, 6)")]
        public decimal TotalSalary { get; set; } //Tổng tiền
        [Column(TypeName = "decimal(19, 6)")]
        public decimal NetSalary { get; set; } //số tiền thực lãnh
        [Column(TypeName = "decimal(19, 6)")]
        public decimal MaxSalary { get; set; } // số tiền thực lãnh tối đa được nhận
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú nếu có
        public double CDM { get; set; } // Công định mức
        public double CTT { get; set; } // Công thực tế
        public DateTime? DateTracking { get; set; }
        public int? UserSign { get; set; }
    }
}
