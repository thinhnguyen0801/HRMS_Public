namespace HNOne.Model.Models
{
    
    public class ShiftChangeModel : AuditableModel
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
        public string? shiftCode2 { get; set; } // ca đổi
        public string? jsonDetail { get; set; } // danh sách chi tiết
    }

    /// <summary>
    /// danh sách chi tiết ngày đổi ca
    /// </summary>
    public class ShiftChange1Model
    {
        public int id { get; set; }
        public int shiftChangeId { get; set; }
        public DateTime dateChange { get; set; }
        public string? remark { get; set; } // ghi chú
        public string? shiftCode1 { get; set; } // ca mặc định
        public string? shiftCode2 { get; set; } // ca đổi
        public DateTime? dateTracking { get; set; }
        public int? userSign { get; set; }
        public string? bgColor { get; set; }
        public string? symbol { get; set; } // kí hiệu
        public bool isDayOff { get; set; } // là ngày nghỉ
        public int holidayId { get; set; } // Rơi vô kì nghỉ lễ nào
    }
}
