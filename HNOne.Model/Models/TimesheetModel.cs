namespace HNOne.Model.Models
{
    public class TimesheetModel
    {
        public long id { get; set; }
        public int employeeId { get; set; } // id nhân viên
        public string? employeeCode { get; set; } // Mã nhân viên
        public int branchId { get; set; } // id chi nhánh
        public int month { get; set; } // tháng công
        public int year { get; set; } // năm công
        public string? shiftCode { get; set; } // ca làm việc lấy từ bảng enum
        public DateTime workingDate { get; set; } // ngày công
        public DateTime startDate { get; set; } // thời gian bắt đầu
        public DateTime endDate { get; set; } // thời gian kết thúc
        public DateTime startBreakTime { get; set; } // thời gian nghỉ bắt đầu
        public DateTime endBreakTime { get; set; } // thời gian nghỉ kết thúc
        public int leaveConfigId { get; set; } // Id ngày nghỉ
        public bool isDayOff { get; set; } // là ngày nghỉ ?
        public double totalWorkingHours { get; set; } // tổng số giờ làm việc

        public DateTime? startDateCurrent { get; set; } // thời gian bắt đầu thực tế
        public DateTime? endDateCurrent { get; set; } // thời gian kết thúc thực tế
        public string? symbolOfHoliday { get; set; }
        public string? symbolOfWeekdayDayOff { get; set; }
        public string? bgColorOfHoliday { get; set; }
        public string? bgColorOfWeekdayDayOff { get; set; }
        public string? shiftPreiod { get; set; } // kỳ công
    }
}
