

namespace HNOne.Model.Models
{
    /// <summary>
    /// Model dành cho phụ lục hợp đồng
    /// </summary>
    public class ContractAppendixModel : AuditableModel
    {
        public int id { get; set; }
        public int contractId { get; set; } // hợp đồng
        public int employeeId { get; set; }
        public string? employeeCode { get; set; }
        public string? employeeName { get; set; }
        public int branchId { get; set; }
        public int timesheetId { get; set; } // ID bảng công
        public string? contractCode { get; set; } // số hợp đồng
        public string? contractAppendixCode { get; set; } // số phụ lục
        public DateTime? dateOfSigning { get; set; } // Ngày kí
        public DateTime? effectiveDate { get; set; } // Ngày áp dụng phụ lục
        public DateTime? deductionDate { get; set; } // Ngày trích nộp
        public int employeeSignatureId { get; set; } // nhân viên kí
        public string? employeeSignatureCode { get; set; }
        public string? employeeSignatureName { get; set; }
        public int departmentId { get; set; } // chức vụ
        public int positionId { get; set; } // chức vụ
        public int titleId { get; set; } // chức danh
        public int placeOfWorkId { get; set; } // nơi làm việc
        public int contractNumber { get; set; } // Phụ lục số mấy rồi
        public string? decisionNo { get; set; } // số chứng từ quyết định
        public bool isActive { get; set; } // áp dụng chưa
        public string? remark { get; set; } // ghi chú
        public string? statusCode { get; set; } // trạng thái lấy từ enum
        public string? statusName { get; set; } // trạng thái lấy từ enum
        public double salaryCoefficient { get; set; } // Hệ số lương
        public decimal totalSalary { get; set; } // Tổng lương
        public decimal netSalary { get; set; } // Tổng lương thực nhận
        public string? authorizationLetter { get; set; } // giấy ủy quyền
        public bool isSalaryAdjustment { get; set; } // điều chỉnh lương
        public bool isCompanyDeduction { get; set; } // cty đóng trích nộp thay
        public bool isCompanyIncomeTax { get; set; } // cty đóng trích nộp thay
        public string? taxTypeCode { get; set; } // loại tính thuế
        public string? departmentName { get; set; } // tên phòng ban
        public string? link { get; set; }
        public string? jsonDetail { get; set; } // danh sách cấu hình tính lương

        #region
        public DateTime? dateOfBirth { get; set; } // ngày sinh
        public string? cIC { get; set; } // số căn cước
        public DateTime? issuanceDateCIC { get; set; } // Ngày cấp CCCD
        public string? placeOfIssuanceCIC { get; set; } // nơi cấp CCCD
        public string? placeOfResidence { get; set; } // địa chỉ thường trú
        public string? titleName { get; set; }
        public string? branchName { get; set; }
        #endregion
    }
}
