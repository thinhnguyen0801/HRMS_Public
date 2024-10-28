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
        public bool IsActive { get; set; }
    }
}
