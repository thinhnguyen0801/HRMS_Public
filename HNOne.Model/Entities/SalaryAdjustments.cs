using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Bảng điều chỉnh lương
    /// </summary>
    [Table("SalaryAdjustments")]
    public class SalaryAdjustments : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Không tự tăng
        public int Id { get; set; }
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
        public int ContractId { get; set; } // ID hợp đồng
        public int SalaryCategoryId { get; set; } // ID loại lương
        [Column(TypeName = "decimal(19, 6)")]
        public decimal Amount { get; set; } // Số tiền
        public double SalaryCoefficient { get; set; } // Hệ số lương
        public bool IsSocialInsurance { get; set; } // BHXH
        public bool IsHealthInsurance { get; set; } // BHYT
        public bool IsAccidentInsurance { get; set; } // BHTN
        public bool IsOccupationalAccidentInsurance { get; set; } // BHTNLD
        public bool IsUnionFee { get; set; } // phí công đoàn
        public bool IsProbationaryPeriod { get; set; } // là thử việc. tính theo %
    }
}
