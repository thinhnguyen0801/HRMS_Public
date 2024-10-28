
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    [Table("Users")]
    public class Users : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int UserId { get; set; }
        [MaxLength(50)]
        public string? UserName { get; set; }
        [MaxLength(500)]
        public string? Password { get; set; }
        [MaxLength(500)]
        public string? DefaultPassword { get; set; } // mật khẩu mặt định
        [MaxLength(250)]
        public string? RefreshToken { get; set; } 
        public DateTime? RefreshTokenExpiryTime { get; set; } // thời hạn hết token
        public int BranchId { get; set; }
        public int EmployeeId { get; set; } // ID nhân viên gắn cho User
        [MaxLength(250)]
        public string? DepartmentIds { get; set; } // danh sách phòng ban sử dụng
        [MaxLength(250)]
        public string? BranchIds { get; set; } // dánh sách chi nhánh được phép sử dụng
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public int PerGroupId { get; set; } // phân quyền nhóm
    }
}
