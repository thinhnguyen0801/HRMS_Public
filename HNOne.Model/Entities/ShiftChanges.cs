using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Chứng từ đề nghị đổi ca làm việc
    /// </summary>
    [Table("ShiftChanges")]
    public sealed class ShiftChanges : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string? VoucherNo { get; set; } // số chứng từ
        public int EmployeeId { get; set; } // nhân viên
        public int EmployeeSignatureId { get; set; } // nhân viên kí
        public DateTime? DateOfSigning { get; set; } // ngày kí
        public int BranchId { get; set; }
        public int DepartmentId { get; set; } // chức vụ
        [MaxLength(50)]
        [Required]
        public string? StatusCode { get; set; }
        public DateTime? FromDate { get; set; } // Ngày bắt đầu
        public DateTime? ToDate { get; set; } // Ngày kết thúc
        [MaxLength(250)]
        public string? Reason { get; set; } // lý do đổi ca
    }

    /// <summary>
    /// Bảng lưu danh sách ngày nghỉ phép
    /// </summary>
    [Table("ShiftChange1s")]
    public sealed class ShiftChange1s
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public int Id { get; set; }
        public int ShiftChangeId { get; set; } // id đăng kí đổi ca
        public DateTime DateChange { get; set; } // ngày nghỉ
        [MaxLength(50)]
        [Required]
        public string? ShiftCode1 { get; set; } // ca mặc định
        [MaxLength(50)]
        [Required]
        public string? ShiftCode2 { get; set; } // ca mặc định
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú
        public DateTime? DateTracking { get; set; }
        public int? UserSign { get; set; }
    }
}
