
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace HNOne.Model.Entities
{
    /// <summary>
    /// Chi nhánh làm việc
    /// </summary>
    [Table("WorkingBranchs")]
    public sealed class WorkingBranchs : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(250)]
        public string? Name { get; set; }
        public int BranchId { get; set; }
        [MaxLength(500)]
        public string? Remark { get; set; }
    }
}
