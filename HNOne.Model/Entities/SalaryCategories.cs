using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Bảng danh mục loại lương
    /// </summary>
    [Table("SalaryCategories")]
    public class SalaryCategories : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(250)]
        public string? Code { get; set; }
        [MaxLength(250)]
        public string? Name { get; set; }
        public int RowOrder { get; set; } // đánh số tt
        public bool IsActive { get; set; }
    }
}
