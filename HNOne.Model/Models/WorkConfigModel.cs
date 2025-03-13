
namespace HNOne.Model.Models
{
    public class WorkConfigModel : AuditableModel
    {
        public int status { get; set; }
        public string? message { get; set; }
        public int id { get; set; }
        public int branchId { get; set; }
        public string? branchName { get; set; }
        public int startDate { get; set; } // ngày bắt đầu chấm công
        public int closingDate { get; set; } // ngày chốt kì công công
        public int closingDate1 { get; set; } // ngày kết thúc chấm công
        public bool isLastDayOfMonth { get; set; } // check vào ngày cuối tháng
        public double totalWorkingDayOfMonth { get; set; } // tổng số ngày làm việc trong tháng
        public bool isWorkingDayExcludeDayOff { get; set; } // tổng số ngày làm việc loại trừ ngày nghỉ
        public double totalWorkingHours { get; set; } // tổng số giờ làm việc trong ngày
        public string? symbolOfWeekdayDayOff { get; set; } // ký hiệu ngày nghỉ trong tuần
        public string? bgColorOfWeekdayDayOff { get; set; } // màu ngày nghỉ trong tuần
        public string? symbolOfHoliday { get; set; } // ký hiệu ngày nghỉ lễ
        public string? bgColorOfHoliday { get; set; } // màu ngày nghỉ lễ
        public string? workConfigType { get; set; } // loại chứa thông tin mặc định hay không
        public string? symbolWorkingDay { get; set; } // ký hiệu ngày làm việc

        public string? symbolOfUnpaidLeave { get; set; } // ký hiệu ngày nghỉ phép không lương
        public string? bgColorOfUnpaidLeave { get; set; } // màu ngày nghỉ nghỉ phép không lương
        public string? symbolOfOvertime { get; set; } // ký hiệu ngày tăng ca
        public string? bgColorOfOvertime { get; set; } // màu ngày tăng ca
        public string? symbolOfLeaveOfAbsence { get; set; } // ký hiệu ngày đăng ký nghỉ
        public string? bgColorOfLeaveOfAbsence { get; set; } // màu ngày ngày đăng ký nghỉ
        #region Thông số dành cho chi tiết
        public int year { get; set; }
        public int month { get; set; } // tháng
        public double totalWorkingDayOfMonthD { get; set; } // công tiêu chuẩn
        public double totalWorkingHoursD { get; set; } // tổng số giờ làm việc
        public int startDateD { get; set; } // ngày bắt đầu chấm công
        public int closingDateD { get; set; } // ngày chốt kì công công
        public int closingDate1D { get; set; } // ngày kết thúc chấm công

        #endregion Thông số dành cho chi tiết
    }
}
