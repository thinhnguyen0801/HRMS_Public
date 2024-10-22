using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Bảng phòng ban
    /// </summary>
    [Table("Departments")]
    public class Departments : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(50)]
        public string? Code { get; set; }
        [MaxLength(250)]
        public string? Name { get; set; }
        public int ManagerId { get; set; } // Id giám đốc
        public int HeadId { get; set; } // Id trưởng phòng
        [MaxLength(100)]
        public string? AssistantManagerIds { get; set; } // có thể có nhiều phó phòng
        [MaxLength(500)]
        public string? Remark { get; set; }
        public bool IsActive { get; set; } = true;
        public int BranchId { get; set; }
    }
}
