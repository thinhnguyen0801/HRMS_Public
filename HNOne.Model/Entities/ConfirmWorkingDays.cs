using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Chứng từ xác nhận ngày công
    /// </summary>
    [Table("ConfirmWorkingDays")]
    public sealed class ConfirmWorkingDays : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string? VoucherNo { get; set; } // số chứng từ
        public int EmployeeId { get; set; } // nhân viên
        public DateTime WorkingDate { get; set; }
        public int EmployeeSignatureId { get; set; } // nhân viên kí
        public DateTime? DateOfSigning { get; set; } // ngày kí
        public int BranchId { get; set; } // chi nhánh
        public int DepartmentId { get; set; } // chức vụ
        [MaxLength(50)]
        [Required]
        public string? StatusCode { get; set; }
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú
    }

    /// <summary>
    /// Bảng lưu danh sách chi tiết ngày xác nhận giờ công thiếu
    /// </summary>
    /// 
    [Table("ConfirmWorkingDay1s")]
    public sealed class ConfirmWorkingDay1s
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public int Id { get; set; }
        public int ConfirmWorkingDayId { get; set; } // id 
        public int EmployeeId { get; set; } // nhân viên
        public DateTime WorkingDate { get; set; } // ngày nghỉ
        public DateTime FromTime { get; set; } // giờ nghỉ bắt đầy
        public DateTime ToTime { get; set; } // Giờ nghỉ kết thúc
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú
        [MaxLength(50)]
        public string? ShiftCode { get; set; } // ca làm việc
        public DateTime? StartTime { get; set; } // thời gian bắt đầu
        public DateTime? EndTime { get; set; } // thời gian kết thúc
        public DateTime? StartBreakTime { get; set; } // thời gian nghỉ bắt đầu
        public DateTime? EndBreakTime { get; set; } // thời gian nghỉ kết thúc
        public double TotalWorkingHours { get; set; } // tổng số giờ làm việc
        public DateTime? StartTimeActual { get; set; } // thời gian bắt đầu
        public DateTime? EndTimeActual { get; set; } // thời gian kết thúc
        public DateTime? StartBreakTimeActual { get; set; } // thời gian nghỉ bắt đầu
        public DateTime? EndBreakTimeActual { get; set; } // thời gian nghỉ kết thúc
        public double TotalWorkingHoursActual { get; set; } // tổng số giờ làm việc
        public double TotalMissingHours { get; set; } // tổng số giờ thiếu
        public DateTime? DateTracking { get; set; }
        public int? UserSign { get; set; }
    }
}
