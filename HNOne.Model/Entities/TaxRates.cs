using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Danh mục mức thuế
    /// </summary>
    [Table("TaxRates")]
    public sealed class TaxRates : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        public int BranchId { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal MinSalary { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal MaxSalary { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal ProgressiveAmount { get; set; } // số tiền lũy tiến
        public double TaxRate { get; set; } // % thuế
        public int TaxBracket { get; set; } // bậc thuế
    }
}
