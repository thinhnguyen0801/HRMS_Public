using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Chứng từ xin nghỉ trong giờ (Đăng kí đi muộn, về sớm)
    /// </summary>
    [Table("LeaveWorkingHours")]
    public sealed class LeaveWorkingHours : Auditable
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
        public int BranchId { get; set; } // chi nhánh
        public int DepartmentId { get; set; } // chức vụ
        [MaxLength(50)]
        [Required]
        public string? StatusCode { get; set; }
        public DateTime? FromDate { get; set; } // Ngày bắt đầu
        public DateTime? ToDate { get; set; } // Ngày kết thúc
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú
        [MaxLength(50)]
        [Required]
        public string? RequestType { get; set; } // loại yêu cầu
        public double TotalHours { get; set; } // tổng số giờ
    }

    /// <summary>
    /// Bảng lưu danh sách phiếu đăng kí xin nghỉ trong giờ
    /// </summary>
    [Table("LeaveWorkingHour1s")]
    public sealed class LeaveWorkingHour1s
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public int Id { get; set; }
        public int LeaveWorkingHourId { get; set; } // id 
        public DateTime WorkingDay { get; set; } // ngày nghỉ
        public DateTime? FromTime { get; set; } // giờ nghỉ bắt đầy
        public DateTime? ToTime { get; set; } // Giờ nghỉ kết thúc
        public double TotalHours { get; set; } // tổng số giờ

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
