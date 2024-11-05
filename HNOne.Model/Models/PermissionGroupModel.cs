
namespace HNOne.Model.Models
{
    public class PermissionGroupModel : AuditableModel
    {
        public int id { get; set; }
        public string? code { get; set; } // Mã nhóm
        public string? name { get; set; } // Tên nhóm
        public bool isActive { get; set; } = true; // hoạt động không
        public string? remark { get; set; } // Mô tả
    }
}
