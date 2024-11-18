using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    [Table("Branchs")]
    public sealed class Branchs : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int BranchId { get; set; }
        [MaxLength(50)]
        public string? BranchCode { get; set; }
        [MaxLength(250)]
        public string? BranchName { get; set; }
        [MaxLength]
        public string? ImgUrl { get; set; }
        [MaxLength(500)]
        public string? Address { get; set; }
        [MaxLength(50)]
        public string? PhoneNumber { get; set; }
        [MaxLength(500)]
        public string? DefaultPassword { get; set; } // mật khẩu mặt định
    }
}
