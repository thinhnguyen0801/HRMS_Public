namespace HNOne.Model.Models
{
    /// <summary>
    /// Điều chỉnh phép năm
    /// </summary>
    public class AdjustedAnnualLeaveRequestModel : AuditableModel
    {
        public int id { get; set; }
        public string? voucherNo { get; set; } // số chứng từ
        public int year { get; set; }
        public int employeeSignatureId { get; set; } // nhân viên kí
        public string? employeeSignatureCode { get; set; }
        public string? employeeSignatureName { get; set; }
        public DateTime? dateOfSigning { get; set; } // ngày kí
        public int branchId { get; set; }
        public string? statusCode { get; set; }
        public string? statusName { get; set; } // trạng thái lấy từ enum
        public string? remark { get; set; } // ghi chú
        public string? jsonDetail { get; set; } // danh sách chi tiết
        public string? link { get; set; }
    }

    public class AdjustedAnnualLeaveRequest1Model
    {
        public int id { get; set; }
        public int adjustedALId { get; set; } // id thông tin điều chỉnh phép
        public int employeeId { get; set; }
        public string? employeeCode { get; set; }
        public string? employeeName { get; set; }
        public DateTime startDate { get; set; } // Ngày áp dụng điều chỉnh phép
        public double numOfAdjustedLeave { get; set; } // Số phép điều chỉnh
        public string? remark { get; set; } // ghi chú cho từng nhân viên
        public DateTime? dateTracking { get; set; }
        public int? userSign { get; set; }
    }
}
