

namespace HNOne.Model.Models
{
    public class OvertimeRequestModel : AuditableModel
    {
        public int id { get; set; }
        public string? voucherNo { get; set; } // số chứng từ
        public int employeeId { get; set; }
        public string? employeeCode { get; set; }
        public string? employeeName { get; set; }
        public int employeeSignatureId { get; set; } // nhân viên kí
        public string? employeeSignatureCode { get; set; }
        public string? employeeSignatureName { get; set; }
        public int branchId { get; set; }
        public string? reason { get; set; } // lý do
        public int departmentId { get; set; } // phòng ban
        public string? departmentName { get; set; }
        public string? statusCode { get; set; } // trạng thái lấy từ enum
        public string? statusName { get; set; } // trạng thái lấy từ enum
        public DateTime? fromDate { get; set; } // Ngày bắt đầu
        public DateTime? toDate { get; set; } // Ngày kết thúc
        public DateTime? dateOfSigning { get; set; } // Ngày kí
        public string? link { get; set; }
        public string? jsonDetail { get; set; } // danh sách chi tiết
        public string? requestType { get; set; } // loại yêu cầu
        public string? requestTypeName { get; set; } // loại yêu cầu
        public string? shiftCode { get; set; } // ca đổi
        public double totalHours { get; set; } // tổng số giờ
    }

    public class OvertimeRequest1Model
    {
        public int id { get; set; }
        public int overtimeRequestId { get; set; }
        public string? shiftCode { get; set; } // ca làm việc
        public DateTime overtimeDate { get; set; } // ngày tăng ca
        public DateTime startTime { get; set; } // giờ vào
        public DateTime endTime { get; set; } // giờ ra
        public DateTime? startBreakTime { get; set; } // giờ bắt đầu nghỉ giữa ca
        public DateTime? endBreakTime { get; set; } // giờ kết thúc nghỉ  ra
        public DateTime? dateTracking { get; set; }
        public int? userSign { get; set; }
        public string? bgColor { get; set; }
        public string? remark { get; set; } // ghi chú
        public string? symbol { get; set; } // kí hiệu
        public bool isDayOff { get; set; } // là ngày nghỉ
        public int holidayId { get; set; } // Rơi vô kì nghỉ lễ nào
        public double totalWorkingHours { get; set; } // tổng số giờ làm việc
    }
}
