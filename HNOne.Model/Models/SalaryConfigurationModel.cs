
namespace HNOne.Model.Models
{
    public class SalaryConfigurationModel : AuditableModel
    {
        public int id { get; set; }
        public int salaryCategoryId { get; set; } // ID loại lương
        public string? salaryCategoryCode { get; set; } // mã loại lương
        public string? salaryCategoryName { get; set; } // tên loại lương
        public int branchId { get; set; }
        public string? branchName { get; set; }
        public bool isActive { get; set; } // áp dụng chưa
        public bool isPersonalIncomeTax { get; set; } // TNCN ?
        public decimal taxLimit { get; set; } // giới hạn thuế
        public bool isSocialInsurance { get; set; } // BHXH
        public bool isHealthInsurance { get; set; } // BHYT
        public bool isAccidentInsurance { get; set; } // BHTN
        public bool isOccupationalAccidentInsurance { get; set; } // BHTNLD
        public bool isUnionFee { get; set; } // phí công đoàn
        public bool isOvertime { get; set; } // tăng ca ?
        public double overtimeCoefficient { get; set; } // hệ số tăng ca
        public bool isNightShift { get; set; } // tăng ca dêm
        public double coefficientNightShift { get; set; } // hệ số ca đêm
        public bool isAllowance { get; set; } // là phụ cấp
        public bool isProbationaryPeriod { get; set; } // là thử việc. tính theo %
        public decimal salaryDefault { get; set; } // Tiền lương mặt định
        public string? salaryCalculateMethod { get; set; } // cách tính lương lấy ở enum
        public string? SalaryCalculateMethodName { get; set; } // cách tính lương lấy ở enum
        public bool isUseOfGradeLevel { get; set; } // có sử dụng ngạch bậc không?
        public decimal amount { get; set; } // số tiền
    }
}
