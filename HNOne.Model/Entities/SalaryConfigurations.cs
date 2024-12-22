using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Danh mục cấu hình thông tin lương
    /// </summary>
    [Table("SalaryConfigurations")]
    public class SalaryConfigurations : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // không tự tăng
        public int Id { get; set; }
        public int SalaryCategoryId { get; set; } // ID loại lương
        public int BranchId { get; set; }
        public bool IsActive { get; set; } // áp dụng chưa
        public bool IsPersonalIncomeTax { get; set; } // TNCN ?

        [Column(TypeName = "decimal(19, 6)")]
        public decimal TaxLimit { get; set; } // giới hạn thuế
        public bool IsSocialInsurance { get; set; } // BHXH
        public bool IsHealthInsurance { get; set; } // BHYT
        public bool IsAccidentInsurance { get; set; } // BHTN
        public bool IsOccupationalAccidentInsurance { get; set; } // BHTNLD
        public bool IsUnionFee { get; set; } // phí công đoàn
        public bool IsOvertime { get; set; } // tăng ca ?
        public double OvertimeCoefficient { get; set; } // hệ số tăng ca
        public bool IsNightShift { get; set; } // tăng ca dêm
        public double CoefficientNightShift { get; set; } // hệ số ca đêm
        public bool IsAllowance { get; set; } // là phụ cấp
        public bool IsPrintContract { get; set; } // in hợp đồng/phụ lục
        public bool IsProbationaryPeriod { get; set; } // là thử việc. tính theo %

        [Column(TypeName = "decimal(19, 6)")]
        public decimal SalaryDefault { get; set; } // Tiền lương mặt định
        [MaxLength(50)]
        public string? SalaryCalculateMethod { get; set; } // cách tính lương lấy ở enum
        public bool IsUseOfGradeLevel { get; set; } // có sử dụng ngạch bậc không?
    }
}
