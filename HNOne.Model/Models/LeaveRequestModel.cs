
namespace HNOne.Model.Models
{
    public class LeaveRequestModel : AuditableModel
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
        public int reasonId { get; set; } // lý do
        public string? reasonName { get; set; } //
        public int departmentId { get; set; } // phòng ban
        public string? departmentName { get; set; }
        public string? statusCode { get; set; } // trạng thái lấy từ enum
        public string? statusName { get; set; } // trạng thái lấy từ enum
        public DateTime? fromDate { get; set; } // Ngày bắt đầu
        public DateTime? toDate { get; set; } // Ngày kết thúc
        public DateTime? dateOfSigning { get; set; } // Ngày kí
        public string? remark { get; set; } // ghi chú
        public string? link { get; set; }
        public double numOfLeave { get; set; } // số phép 
        public double numOfLeaveOld { get; set; } // số phép năm củ
        public double numOfLeaveOldPaid { get; set; } // phép của năm củ đã thanh toán
        public DateTime expiryALOldDate { get; set; } // Ngày hết hạn sử dụng phép năm củ
        public double numOfLeaveTotal { get; set; } // 
        public double numOfLeaveLevel { get; set; } // số phép tăng theo thâm niên
        public double numOfLeaveUsed { get; set; } // số phép sử dụng
        public double numOfLeavePending { get; set; } // phép tạo mới/chờ duyệt
        public double numOfAdjustedLeave { get; set; } // số phép đã điều chỉnh
        public double numOfLeaveRemaining { get; set; } // số phép còn lại
        public string? jsonDetail { get; set; } // danh sách chi tiết
        public double totalHours { get; set; } // tổng số giờ nghỉ
        #region dành cho xin nghỉ phép trong giờ
        public string? requestType { get; set; } // loại yêu cầu
        public string? requestTypeName { get; set; } // loại yêu cầu
        public DateTime? fromDateTime { get; set; } // Ngày bắt đầu
        public string? employeeListCode { get; set; }
        public string? employeeListName { get; set; }
        #endregion
    }

    // danh sách chi tiết ngày nghỉ
    public class LeaveRequest1Model
    {
        public int id { get; set; }
        public int leaveRequestId { get; set; }
        public DateTime dateOff { get; set; }
        public bool isMorningBreak { get; set; } // nghỉ buổi sáng
        public bool isAfternoonBreak { get; set; } // nghỉ buổi chiều
        public string? remark { get; set; } // ghi chú
        public DateTime? dateTracking { get; set; }
        public int? userSign { get; set; }
        public string? bgColor { get; set; }
        public string? symbol { get; set; } // kí hiệu
        public bool isDayOff { get; set; } // là ngày nghỉ
        public int holidayId { get; set; } // Rơi vô kì nghỉ lễ nào
        public DateTime? fromTime { get; set; } // Ngày bắt đầu
        public DateTime? toTime { get; set; } // Ngày kết thúc
        public double totalLeaveDays { get; set; }
        public double totalHours { get; set; } // tổng số giờ làm việc
        public string? shiftCode { get; set; } // ca làm việc lấy từ bảng enum
        public string? shiftName { get; set; } // ca làm việc lấy từ bảng enum
        public DateTime? startDate { get; set; } // thời gian bắt đầu
        public DateTime? endDate { get; set; } // thời gian kết thúc
        public DateTime? startBreakTime { get; set; } // thời gian nghỉ bắt đầu
        public DateTime? endBreakTime { get; set; } // thời gian nghỉ kết thúc
    }
}
