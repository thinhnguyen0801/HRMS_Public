using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace HNOne.Model.Entities
{
    /// <summary>
    /// Chứng từ đề nghị điều chỉnh phép năm của nhân viên
    /// </summary>
    [Table("AdjustedAnnualLeaveRequests")]
    public sealed class AdjustedAnnualLeaveRequests : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string? VoucherNo { get; set; } // số chứng từ
        public int Year { get; set; }
        public int EmployeeSignatureId { get; set; } // nhân viên kí
        public DateTime? DateOfSigning { get; set; } // ngày kí
        public int BranchId { get; set; }
        [MaxLength(50)]
        [Required]
        public string? StatusCode { get; set; }
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú
    }

    /// <summary>
    /// Chứng từ đề nghị điều chỉnh phép năm của nhân viên
    /// </summary>
    [Table("AdjustedAnnualLeaveRequest1s")]
    public sealed class AdjustedAnnualLeaveRequest1s
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public int Id { get; set; }
        public int AdjustedALId { get; set; } // id thông tin điều chỉnh phép
        public int EmployeeId { get; set; } // nhân viên
        public DateTime StartDate { get; set; } // Ngày áp dụng điều chỉnh phép
        public double NumOfAdjustedLeave { get; set; } // Số phép điều chỉnh
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú cho từng nhân viên
        public DateTime? DateTracking { get; set; }
        public int? UserSign { get; set; }
    }
}
