
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Bảng nhóm quyền
    /// </summary>
    [Table("PermissionGroups")]
    public sealed class PermissionGroups : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(50)]
        public string? Code { get; set; } // Mã nhóm
        [MaxLength(250)]
        public string? Name { get; set; } // Tên nhóm
        public bool IsActive { get; set; } // hoạt động không
        [MaxLength(250)]
        public string? Remark { get; set; } // Mô tả
        public int BranchId { get; set; } // nhóm quyền thuộc chi nhánh
    }
}
