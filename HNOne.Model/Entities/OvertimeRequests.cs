using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Chứng từ đề nghị tăng ca
    /// </summary>
    [Table("OvertimeRequests")]
    public sealed class OvertimeRequests : Auditable
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
        [MaxLength(50)]
        [Required]
        public string? RequestType { get; set; } // loại yêu cầu
        public DateTime? FromDate { get; set; } // Ngày bắt đầu
        public DateTime? ToDate { get; set; } // Ngày kết thúc
        [MaxLength(250)]
        public string? Reason { get; set; } // lý do tăng ca
    }

    /// <summary>
    /// Bảng lưu danh sách ngày nghỉ phép
    /// </summary>
    [Table("OvertimeRequest1s")]
    public sealed class OvertimeRequest1s
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public int Id { get; set; }
        public int OvertimeRequestId { get; set; }
        [MaxLength(50)]
        [Required]
        public string? ShiftCode { get; set; } // ca làm việc
        public DateTime OvertimeDate { get; set; } // ngày tăng ca
        public DateTime StartTime { get; set; } // giờ vào
        public DateTime EndTime { get; set; } // giờ ra
        public DateTime? StartBreakTime { get; set; } // giờ bắt đầu nghỉ giữa ca
        public DateTime? EndBreakTime { get; set; } // giờ kết thúc nghỉ  ra
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú
        public bool IsDayOff { get; set; } // là ngày nghỉ
        [MaxLength(250)]
        public string? BgColor { get; set; } // màu line
        [MaxLength(250)]
        public string? Symbol { get; set; } // ký hiệu
        public double TotalWorkingHours { get; set; } // tổng số giờ làm việc
        public DateTime? DateTracking { get; set; }
        public int? UserSign { get; set; }
    }
}
