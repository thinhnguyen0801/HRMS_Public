
namespace HNOne.Model.Models
{
    /// <summary>
    /// Model dành cho báo cáo tổng hợp
    /// </summary>
    public class SummaryReportModel
    {
        public int totalEmployees {  get; set; } // tổng nhân viên
        public int totalOfficialEmployees {  get; set; } // tổng nhân viên chính thức
        public int totalProbationaryEmployees {  get; set; } // tổng nhân viên thử việc
        public int totalEmployeesOnMaternityLeave { get; set; } // tổng nhân viên nghỉ thai sản
        public int totalOtherEmployees { get; set; } // tổng nhân viên khác

        public int totalLeaveRequests { get; set; } // tổng nhân viên xin nghỉ
        public int totalOvertimes { get; set; } // tổng nhân viên xin nghỉ
        public int totalLateArrivalsAndEarlyLeaves { get; set; } // tổng nhân viên đi muộn về sớm
        public int totalMissingTimeAttendance { get; set; } // tổng nhân viên quên chấm công

        public int totalEmployeesEndingProbationSoon { get; set; } // tổng nhân viên sắp hết hạn thử việc
        public string? dayEmployeesEndingProbationSoon { get; set; } // tổng nhân viên sắp hết hạn thử việc
        public int totalEmployeesOnMaternityLeaveSoon { get; set; } // tổng nhân viên nghỉ thai sản sắp đến hạn
        public string? dayEmployeesOnMaternityLeaveSoon { get; set; } // tổng nhân viên nghỉ thai sản sắp đến hạn
        public int totalBirthdays { get; set; } // tổng nhân viên có sinh nhật
        public string? dayBirthdays { get; set; } // tổng nhân viên có sinh nhật
        public int totalUpcomingWorkAnniversaries { get; set; } // tổng nhân viên sắp kỷ niệm ngày vào làm
        public string? dayUpcomingWorkAnniversaries { get; set; } // tổng nhân viên sắp kỷ niệm ngày vào làm
        
        public int totalContractSoon { get; set; } // hợp đồng sắp hết hạn
        public string? dayContractSoon { get; set; } // hợp đồng sắp hết hạn
        public int totalContractExpired { get; set; } // hợp đồng hết hạn
    }
}
