using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Dữ liệu công của từng nhân viên trong tháng
    /// </summary>
    [Table("AttendanceSummarys")]
    public sealed class AttendanceSummarys : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public long Id { get; set; }
        public int EmployeeId { get; set; } // id nhân viên
        [MaxLength(50)]
        [Required]
        public string? EmployeeCode { get; set; } // Mã nhân viên
        [MaxLength(250)]
        public string? EmployeeName { get; set; } // Tên nhân viên
        
        public int BranchId { get; set; } // id chi nhánh
        public int DepartmentId { get; set; }

        [MaxLength(50)]
        public string? DepartmentCode { get; set; } // Mã phòng ban
        [MaxLength(250)]
        public string? DepartmentName { get; set; } // Tên phòng ban

        public int PositionId { get; set; }
        [MaxLength(50)]
        public string? PositionCode { get; set; } // Mã phòng ban
        [MaxLength(250)]
        public string? PositionName { get; set; } // Tên phòng ban

        public int TitleId { get; set; }
        [MaxLength(50)]
        public string? TitleCode { get; set; } // Mã phòng ban
        [MaxLength(250)]
        public string? TitleName { get; set; } // Tên phòng ban

        [MaxLength(50)]
        [Required]
        public string? ShiftCode { get; set; } // ca làm việc lấy từ bảng enum
        public int Month { get; set; } // tháng công
        public int Year { get; set; } // năm công
        public double TNC { get; set; } // tổng ngày công
        public double CDM { get; set; } // công định mức của tháng
        public double CTT { get; set; } // công thực tế
        public double NL { get; set; } // những ngày nghỉ lễ
        public double NPN { get; set; } // nghỉ phép năm
        public double NCD { get; set; } // nghỉ chế độ
        public double NPKL { get; set; } // nghỉ phép không lương
        public double NB { get; set; } // nghỉ bù
        public double NKP { get; set; } // nghỉ không phép
        public double NNV { get; set; } // nghỉ ngừng việc
        public double CTPC { get; set; } // số công tính phụ cấp
        public double TGDLTVS { get; set; } // thời gian đi trễ về sớm
        public double SLDLTVS { get; set; } // số lần đi trễ về sớm
        public double SGT { get; set; } // số giờ thiếu
        public double SGTC { get; set; } // số giờ trừ công
        public double GCTC { get; set; } // giờ công của 1 ngày
        public double TGTC { get; set; } // số giờ tăng ca
        public double SGTCTC { get; set; } // số giờ tăng ca tiêu chuẩn
        public double SGTCTT { get; set; } // số giờ tăng ca của tháng trước
        public double SGTCKT { get; set; } // số giờ tăng ca được chuyển sang tháng tiếp theo
        public bool IsLocked { get; set; } // chốt chưa, chốt những ai
        [MaxLength(250)]
        public string? N01 { get; set; }
        [MaxLength(250)]
        public string? N02 { get; set; }
        [MaxLength(250)]
        public string? N03 { get; set; }
        [MaxLength(250)]
        public string? N04 { get; set; }
        [MaxLength(250)]
        public string? N05 { get; set; }
        [MaxLength(250)]
        public string? N06 { get; set; }
        [MaxLength(250)]
        public string? N07 { get; set; }
        [MaxLength(250)]
        public string? N08 { get; set; }
        [MaxLength(250)]
        public string? N09 { get; set; }
        [MaxLength(250)]
        public string? N10 { get; set; }
        [MaxLength(250)]
        public string? N11 { get; set; }
        [MaxLength(250)]
        public string? N12 { get; set; }
        [MaxLength(250)]
        public string? N13 { get; set; }
        [MaxLength(250)]
        public string? N14 { get; set; }
        [MaxLength(250)]
        public string? N15 { get; set; }
        [MaxLength(250)]
        public string? N16 { get; set; }
        [MaxLength(250)]
        public string? N17 { get; set; }
        [MaxLength(250)]
        public string? N18 { get; set; }
        [MaxLength(250)]
        public string? N19 { get; set; }
        [MaxLength(250)]
        public string? N20 { get; set; }
        [MaxLength(250)]
        public string? N21 { get; set; }
        [MaxLength(250)]
        public string? N22 { get; set; }
        [MaxLength(250)]
        public string? N23 { get; set; }
        [MaxLength(250)]
        public string? N24 { get; set; }
        [MaxLength(250)]
        public string? N25 { get; set; }
        [MaxLength(250)]
        public string? N26 { get; set; }
        [MaxLength(250)]
        public string? N27 { get; set; }
        [MaxLength(250)]
        public string? N28 { get; set; }
        [MaxLength(250)]
        public string? N29 { get; set; }
        [MaxLength(250)]
        public string? N30 { get; set; }
        [MaxLength(250)]
        public string? N31 { get; set; }
    }

    /// <summary>
    /// Dữ liệu chốt công chi tiết
    /// </summary>
    [Table("AttendanceSummary1s")]
    public sealed class AttendanceSummary1s
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public long Id { get; set; }
        public int EmployeeId { get; set; } // id nhân viên
        [MaxLength(50)]
        [Required]
        public string? EmployeeCode { get; set; } // Mã nhân viên
        [MaxLength(250)]
        public string? EmployeeName { get; set; } // Tên nhân viên
        public int BranchId { get; set; } // id chi nhánh
        public int DepartmentId { get; set; }

        [MaxLength(50)]
        public string? DepartmentCode { get; set; } // Mã phòng ban
        [MaxLength(250)]
        public string? DepartmentName { get; set; } // Tên phòng ban

        public int PositionId { get; set; }
        [MaxLength(50)]
        public string? PositionCode { get; set; } // Mã phòng ban
        [MaxLength(250)]
        public string? PositionName { get; set; } // Tên phòng ban

        public int TitleId { get; set; }
        [MaxLength(50)]
        public string? TitleCode { get; set; } // Mã phòng ban
        [MaxLength(250)]
        public string? TitleName { get; set; } // Tên phòng ban

        [MaxLength(50)]
        [Required]
        public string? ShiftCode { get; set; } // ca làm việc lấy từ bảng enum
        public int Month { get; set; } // tháng công
        public int Year { get; set; } // năm công
        [MaxLength(250)]
        public string? AttendanceSheetCode { get; set; } // ID bảng công
        public DateTime WorkingDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartBreakTime { get; set; }
        public DateTime EndBreakTime { get; set; }
        public double TotalWorkingHours { get; set; }
        public double TotalWorkingDayOfMonth { get; set; }

        public DateTime? StartDateActual { get; set; }
        public DateTime? EndDateActual { get; set; }
        public DateTime? StartDateConfirmActual { get; set; }
        public DateTime? EndDateConfirmActual { get; set; }
        public DateTime? StartBreakTimeActual { get; set; }
        public DateTime? EndBreakTimeActual { get; set; }
        public double TotalWorkingHoursActual { get; set; }
        public double SGT { get; set; } // số giờ thiếu
        [MaxLength(50)]
        public string? Symbol { get; set; } // kí hiệu ngày nghỉ
        [MaxLength(250)]
        public string? BgColor { get; set; } // tô màu nếu là ngày nghỉ để chốt công loại ngày nghỉ
        public double IsDayOff { get; set; } // là ngày nghỉ
        public int LeaveConfigId { get; set; } // id bảng cấu hình ngày nghỉ
        public int HolidayId { get; set; } // id bảng ngày nghỉ
        public int WorkConfigId { get; set; } // 
        public int LeaveRequestId { get; set; } // phiếu đề nghị nghỉ phép
        public int ReasonId { get; set; } // Id của lý do nghỉ phép
        public int OvertimeRequesttId { get; set; } // phiếu đề nghị tăng ca
        public double TotalOvertimeHours { get; set; } // Tổng số giờ tăng ca
        public int LeaveWorkingHourId { get; set; } // đăng kí đi muộn về sớm
        public double TotalLeaveWorkingHours { get; set; } // Tổng số giờ đi trễ về sớm
        public int ConfirmWorkingDayId { get; set; } // ID đăng kí xác nhận giờ công
        [MaxLength(50)]
        public string? VoucherNo { get; set; } // Chứng từ liên quan
        public int DocEntry { get; set; } // Mã docentry
        [MaxLength(250)]
        public string? ObjType { get; set; } // loại chứng từ

        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú
        public DateTime? CreateDate { get; set; }
        public int? UserSign { get; set; }
    }
}
