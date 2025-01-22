
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// bảng phân quyền cho nhóm
    /// </summary>
    [Table("GroupAccessControls")]
    public sealed class GroupAccessControls : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //tự tăng
        public int Id { get; set; }
        public int GroupId { get; set; } // nhóm người dùng
        public int EventId { get; set; }
        public bool IsAllow { get; set; }
    }

    [Table("DataPermissions")]
    public sealed class DataPermissions : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //tự tăng
        public int Id { get; set; }
        public int GroupId { get; set; } // nhóm người dùng

        [MaxLength(50)]
        public string? Type { get; set; }
        
        [MaxLength(50)]
        public string? Code { get; set; }
        public bool IsAllow { get; set; }
    }
}
