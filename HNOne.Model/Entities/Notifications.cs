using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    [Table("Notifications")]
    public sealed class Notifications
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Tự tăng
        public int Id { get; set; }
        public int BranchId { get; set; }
        public int DocEntry { get; set; }
        [MaxLength(250)]
        [Required]
        public string? VoucherNo { get; set; } // mã chứng từ
        [MaxLength(150)]
        [Required]
        public string? ObjType { get; set; } // loại chứng từ
        [MaxLength(50)]
        [Required]
        public string? StatusCode { get; set; } // trạng thái phê duyệt
        public int EmployeeId { get; set; } // gửi tới ai
        [MaxLength(500)]
        public string? Message { get; set; } // thông báo là gì
        public bool IsView {  get; set; } // đã xem chưa
        public DateTime? CreateDate { get; set; } // ngày giờ tạo
        public int? UserSign { get; set; } // người tạo
    }
}
