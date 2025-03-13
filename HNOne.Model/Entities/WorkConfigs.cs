using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HNOne.Model.Entities
{
    [Table("WorkConfigs")]
    public sealed class WorkConfigs : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Không tự tăng
        public int Id { get; set; }
        public int BranchId { get; set; }
        public int StartDate { get; set; } // ngày bắt đầu chấm công
        public int ClosingDate { get; set; } // ngày chốt kì công công
        public int ClosingDate1 { get; set; } // ngày kết thúc chấm công
        public bool IsLastDayOfMonth { get; set; } // check vào ngày cuối tháng
        public double TotalWorkingDayOfMonth { get; set; } // tổng số ngày làm việc trong tháng
        public bool IsWorkingDayExcludeDayOff { get; set; } // tổng số ngày làm việc loại trừ ngày nghỉ
        public double TotalWorkingHours { get; set; } // tổng số giờ làm việc trong ngày
        [MaxLength(50)]
        [Required]
        public string? SymbolWorkingDay { get; set; } // ký hiệu ngày làm việc
        [MaxLength(50)]
        [Required]
        public string? SymbolOfWeekdayDayOff { get; set; } // ký hiệu ngày nghỉ trong tuần
        [MaxLength(250)]
        public string? BgColorOfWeekdayDayOff { get; set; } // màu ngày nghỉ trong tuần
        [MaxLength(50)]
        [Required]
        public string? SymbolOfHoliday { get; set; } // ký hiệu ngày nghỉ lễ
        [MaxLength(250)]
        public string? BgColorOfHoliday { get; set; } // màu ngày nghỉ lễ

        [MaxLength(50)]
        [Required]
        public string? SymbolOfUnpaidLeave { get; set; } // ký hiệu ngày nghỉ phép không lương
        [MaxLength(250)]
        public string? BgColorOfUnpaidLeave { get; set; } // màu ngày nghỉ nghỉ phép không lương

        [MaxLength(50)]
        [Required]
        public string? SymbolOfOvertime { get; set; } // ký hiệu ngày tăng ca
        [MaxLength(250)]
        public string? BgColorOfOvertime { get; set; } // màu ngày tăng ca

        [MaxLength(50)]
        [Required]
        public string? SymbolOfLeaveOfAbsence { get; set; } // ký hiệu ngày đăng ký nghỉ
        [MaxLength(250)]
        public string? BgColorOfLeaveOfAbsence { get; set; } // màu ngày ngày đăng ký nghỉ

        [MaxLength(50)]
        [Required]
        public string? WorkConfigType { get; set; } // // chia cho header hay là chi tiết
        #region Thông số dành cho chi tiết
        public int Year { get; set; }
        public int Month { get; set; } // tháng
        public double TotalWorkingDayOfMonthD { get; set; } // công tiêu chuẩn
        public double TotalWorkingHoursD { get; set; } // tổng số giờ làm việc
        public int StartDateD { get; set; } // ngày bắt đầu chấm công
        public int ClosingDateD { get; set; } // ngày chốt kì công công
        public int ClosingDate1D { get; set; } // ngày kết thúc chấm công

        #endregion Thông số dành cho chi tiết
    }
}
