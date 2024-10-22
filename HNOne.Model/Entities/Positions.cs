
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Bảng chức vụ
    /// </summary>
    [Table("Positions")]
    public class Positions : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(50)]
        public string? Code { get; set; }
        [MaxLength(250)]
        public string? Name { get; set; }
        [MaxLength(50)]
        public string? LevelCode { get; set; } // cấp độ
        [MaxLength(500)]
        public string? Remark { get; set; }
        public bool IsActive { get; set; } = true;
        public int BranchId { get; set; }
    }
}
