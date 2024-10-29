namespace HNOne.Model.Models
{
    public class ContractModel
    {
        public int id { get; set; }
        public int employeeId { get; set; }
        public string? employeeCode { get; set; }
        public string? employeeName { get; set; }
        public int branchId { get; set; }
        public int timesheetId { get; set; } // ID bảng công
        public string? contractCode { get; set; }
        public DateTime? startDate { get; set; } // Ngày bắt đầu
        public DateTime? endDate { get; set; } // Ngày kết thúc
        public DateTime? dateOfSigning { get; set; } // Ngày kí
        public int employeeSignatureId { get; set; } // nhân viên kí
        public int contractTypeId { get; set; } // Loại hợp đồng
        public int positionId { get; set; } // chức vụ
        public int titleId { get; set; } // chức danh
        public int contractNumber { get; set; } // kí lần thứ mấy rồi
        public string? remark { get; set; } // ghi chú
        public string? statusCode { get; set; } // trạng thái lấy từ enum
        public double? salaryCoefficient { get; set; } // Hệ số lương
        public decimal totalSalary { get; set; } // Tổng lương
        public decimal netSalary { get; set; } // Tổng lương thực nhận
        public double numberOfMonths { get; set; } // số tháng
        public int numberOfDaysReduced { get; set; } // số ngày giảm
        public string? decisionNo { get; set; } // số chứng từ quyết định
        public bool isActive { get; set; } // áp dụng chưa
        public int placeOfWorkId { get; set; } // nơi làm việc
    }
}
