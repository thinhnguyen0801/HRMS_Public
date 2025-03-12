using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    [Table("ReasonCategories")]
    public class ReasonCategories : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(250)]
        public string? Name { get; set; }
        [MaxLength(50)]
        public string? Type { get; set; } // loại lý do
        [MaxLength(250)]
        public string? Value { get; set; } // config nếu có
        [MaxLength(250)]
        public string? Value1 { get; set; } // config nếu có 1
        [MaxLength(250)]
        public string? Value2 { get; set; } // config nếu có 2
        public bool IsActive { get; set; }
        public int BranchId { get; set; }

    }
}
