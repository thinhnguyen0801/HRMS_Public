using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model.Models
{
    public class ConfirmWorkingDayModel : AuditableModel
    {
        public int id { get; set; }
        public string? voucherNo { get; set; } // số chứng từ
        public int employeeId { get; set; }
        public string? employeeCode { get; set; }
        public string? employeeName { get; set; }
        public DateTime workingDate { get; set; }
        public int employeeSignatureId { get; set; } // nhân viên kí
        public string? employeeSignatureCode { get; set; }
        public string? employeeSignatureName { get; set; }
        public int branchId { get; set; }
        public int departmentId { get; set; } // phòng ban
        public string? departmentName { get; set; }
        public string? statusCode { get; set; } // trạng thái lấy từ enum
        public string? statusName { get; set; } // trạng thái lấy từ enum
        public DateTime? dateOfSigning { get; set; } // Ngày kí
        public string? remark { get; set; } // ghi chú
        public string? link { get; set; }
        public string? jsonDetail { get; set; } // danh sách chi tiết
    }

    public class ConfirmWorkingDay1Model
    {
        public int id { get; set; }
        public int confirmWorkingDayId { get; set; }
        public DateTime workingDate { get; set; }
        public DateTime? fromTime { get; set; } // Ngày bắt đầu sáng
        public DateTime? toTime { get; set; } // Ngày kết thúc chiều
        public string? remark { get; set; } // ghi chú
        public string? shiftCode { get; set; } // ca làm việc
        public DateTime? startTime { get; set; } // thời gian bắt đầu
        public DateTime? endTime { get; set; } // thời gian kết thúc
        public DateTime? startBreakTime { get; set; } // thời gian nghỉ bắt đầu
        public DateTime? endBreakTime { get; set; } // thời gian nghỉ kết thúc
        public double totalWorkingHours { get; set; } // tổng số giờ làm việc
        public DateTime? startTimeActual { get; set; } // thời gian bắt đầu
        public DateTime? endTimeActual { get; set; } // thời gian kết thúc
        public DateTime? startBreakTimeActual { get; set; } // thời gian nghỉ bắt đầu
        public DateTime? endBreakTimeActual { get; set; } // thời gian nghỉ kết thúc
        public double totalWorkingHoursActual { get; set; } // tổng số giờ làm việc
        public double totalMissingHours { get; set; } // tổng số giờ thiếu
        public DateTime? dateTracking { get; set; }
        public int? userSign { get; set; }

    }
}
