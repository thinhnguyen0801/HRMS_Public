
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
        public string? reasonCode { get; set; } // lý do lấy từ enum
        public string? reasonName { get; set; } // lý do lấy từ enum
        public int departmentId { get; set; } // chức vụ
        public string? statusCode { get; set; } // trạng thái lấy từ enum
        public string? statusName { get; set; } // trạng thái lấy từ enum
        public DateTime? fromDate { get; set; } // Ngày bắt đầu
        public DateTime? toDate { get; set; } // Ngày kết thúc
        public string? remark { get; set; } // ghi chú
        public string? link { get; set; }
        public int numOfLeave { get; set; }
        public int numOfLeaveOld { get; set; }
        public int numOfLeaveTotal { get; set; }
    }

    // danh sách chi tiết ngày nghỉ
    public class LeaveRequest1Model
    {
        public int leaveRequestId { get; set; }
        public DateTime dateOff { get; set; }
        public bool isMorningBreak { get; set; } // nghỉ buổi sáng
        public bool isAfternoonBreak { get; set; } // nghỉ buổi chiều
        public string? remark { get; set; } // ghi chú
        public DateTime? dateTracking { get; set; }
        public int? userSign { get; set; }

    }
}
