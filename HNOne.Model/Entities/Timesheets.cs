using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Dữ liệu công của từng nhân viên trong tháng
    /// </summary>
    [Table("Timesheets")]
    public sealed class Timesheets : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public long Id { get; set; }
        public int EmployeeId { get; set; } // id nhân viên
        [MaxLength(50)]
        [Required]
        public string? EmployeeCode { get; set; } // Mã nhân viên
        public int BranchId { get; set; } // id chi nhánh
        public int Month { get; set; } // tháng công
        public int Year { get; set; } // năm công
        [MaxLength(50)]
        [Required]
        public string? ShiftCode { get; set; } // ca làm việc lấy từ bảng enum
        public DateTime WorkingDate { get; set; } // ngày công
        public DateTime StartDate { get; set; } // thời gian bắt đầu
        public DateTime EndDate { get; set; } // thời gian kết thúc
        public DateTime StartBreakTime { get; set; } // thời gian nghỉ bắt đầu
        public DateTime EndBreakTime { get; set; } // thời gian nghỉ kết thúc
        public int LeaveConfigId { get; set; } // Id ngày nghỉ
        public bool IsDayOff { get; set; } // là ngày nghỉ ?
        public double TotalWorkingHours { get; set; } // tổng số giờ làm việc
    }
}
