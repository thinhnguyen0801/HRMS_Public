using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Danh mục cấu hình thông tin lương
    /// </summary>
    [Table("SalaryParameters")]
    public sealed class SalaryParameters : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // không tự tăng
        public int Id { get; set; }
        public int BranchId { get; set; }
        public bool IsActive { get; set; } // áp dụng chưa
        [Column(TypeName = "decimal(19, 6)")]
        public decimal TaxSalary { get; set; } // lương chịu thuế
        [Column(TypeName = "decimal(19, 6)")]
        public decimal TaxSalaryProbationary { get; set; } // lương chịu thuế thử việc
        [Column(TypeName = "decimal(19, 6)")]
        public decimal SalaryFamilyCircumstanceDeduction { get; set; } // mức giảm trừ gia cảnh
        public DateTime? FromDate { get; set; } // hiệu lực từ ngày
        public DateTime? ToDate { get; set; } // hiệu lực đến ngày
    }
}
