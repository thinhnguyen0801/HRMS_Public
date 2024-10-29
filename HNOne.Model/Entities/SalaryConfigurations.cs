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
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // tự tăng
        public int Id { get; set; }
        [MaxLength(50)]
        public string? Code { get; set; }
        [MaxLength(250)]
        public string? Name { get; set; }
        public string? BranchId { get; set; }
        public bool IsActive { get; set; } // áp dụng chưa
        public bool IsCalculatePersonalIncomeTax { get; set; } // Tính thuế TNCN ?
        public decimal TaxLimit { get; set; } // giới hạn thuế
        public bool BHXH { get; set; }
        public bool BHYT { get; set; }
        public bool BHTN { get; set; }
        public bool BHTNLD { get; set; }
        public double OvertimeCoefficient { get; set; } // hệ số tăng ca
        public int SalaryCalculateMethod { get; set; } // cách tính lương
    }
}
