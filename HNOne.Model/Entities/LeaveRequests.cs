using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Chứng từ đề nghị nghỉ phép
    /// </summary>
    [Table("LeaveRequests")]
    public sealed class LeaveRequests : Auditable
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
        public int ReasonId { get; set; } // lý do
        public int DepartmentId { get; set; } // chức vụ
        [MaxLength(50)]
        [Required]
        public string? StatusCode { get; set; }
        public DateTime? FromDate { get; set; } // Ngày bắt đầu
        public DateTime? ToDate { get; set; } // Ngày kết thúc
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú
    }

    /// <summary>
    /// Bảng lưu danh sách ngày nghỉ phép
    /// </summary>
    [Table("LeaveRequest1s")]
    public sealed class LeaveRequest1s
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public int Id { get; set; }
        public int LeaveRequestId { get; set; } // id 
        public DateTime DateOff { get; set; } // ngày nghỉ
        public bool IsMorningBreak { get; set; } // nghỉ buổi sáng
        public bool IsAfternoonBreak { get; set; } // nghỉ buổi chiều
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú
        public bool IsDayOff { get; set; } // là ngày nghỉ
        [MaxLength(250)]
        public string? BgColor { get; set; } // màu line
        [MaxLength(250)]
        public string? Symbol { get; set; } // ký hiệu
        public int HolidayId { get; set; } // Rơi vô kì nghỉ lễ nào bảng HolidayCatagories
        public DateTime? DateTracking { get; set; }
        public int? UserSign { get; set; }
    }
}
