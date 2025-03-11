namespace HNOne.Model.Models
{
    /// <summary>
    /// Model dành cho tất cả các báo cáo lương
    /// </summary>
    public class SalaryReportModel
    {
        public int status { get; set; }
        public string? message { get; set; }
        public int id { get; set; }
        public int employeeId { get; set; }
        public string? employeeCode { get; set; }
        public string? employeeName { get; set; }
        public int branchId { get; set; }
        public string? branchCode { get; set; }
        public string? branchName { get; set; }
        public int departmentId { get; set; } // phòng ban
        public string? departmentCode { get; set; }
        public string? departmentName { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public decimal basicSalary { get; set; } // Lương cơ bản
        public decimal negotiatedSalary { get; set; } // Lương thỏa thuận
        public double salaryCoefficient { get; set; } // Hệ số lương
        public decimal convertedNegotiatedSalary { get; set; } // Lương thỏa thuận đã quy đổi với hệ số lương
        public decimal totalAllowance { get; set; } // tổng phụ cấp
        public decimal totalTax { get; set; } // tổng thuế
        public decimal totalInsurance { get; set; } // tổng bảo hiểm
        public decimal totalUnionFee { get; set; } // tổng công đoàn phí
        public decimal totalOvertime { get; set; } // tổng tăng ca
        public decimal totalGross { get; set; } // tổng lương trước thuế
        public decimal totalNet { get; set; } // tổng tăng ca
        public decimal totalSalary { get; set; } // lương
    }
}
