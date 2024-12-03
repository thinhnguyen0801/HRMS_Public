
namespace HNOne.Model.Models
{
    public class ApprovalModel
    {
        public int id { get; set; }
        public int branchId { get; set; }
        public string? branchCode { get; set; }
        public string? branchName { get; set; }
        public int docEntry { get; set; }
        public int contractId { get; set; }
        public string? voucherNo { get; set; }
        public string? objType { get; set; } // loại chứng từ
        public string? objTypeName { get; set; } // loại chứng từ
        public string? statusCode { get; set; } // trạng thái phê duyệt
        public string? statusName { get; set; } // trạng thái lấy từ enum
        public DateTime? createDate { get; set; } // ngày giờ tạo
        public int? userSign { get; set; } // người tạo
        public string? employeeCode { get; set; } // mã nhân viên gửi
        public string? employeeName { get; set; } // tên nhân viên gửi
        public DateTime? updateDate { get; set; } // ngày giờ cập nhật
        public int? userSign2 { get; set; } // người cập nhật
        public DateTime? dateTracking { get; set; }
        public int employeeSignatureId { get; set; } // nhân viên kí
        public string? employeeSignatureCode { get; set; }
        public string? employeeSignatureName { get; set; }
        public string? remark { get; set; } // ghi chú chứng từ
        public string? approvalRemark { get; set; } // ghi chú phê duyệt
        public string? departmentName { get; set; } // phòng ban
        public string? link { get; set; } // phòng ban
    }
}
