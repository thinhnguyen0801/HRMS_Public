using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Bảng lưu thông tin lịch sử chứng từ
    /// </summary>
    [Table("DocumentHistories")]
    public sealed class DocumentHistories
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public int Id { get; set; }
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
        public int DocEntry { get; set; }
        [MaxLength(150)]
        [Required]
        public string? ObjType { get; set; } // loại chứng từ
        [MaxLength(50)]
        [Required]
        public string? StatusCode { get; set; } // trạng thái mới
        [MaxLength(50)]
        [Required]
        public string? StatusCodePre { get; set; } // trạng thái kế đó
        public DateTime? CreateDate { get; set; } // ngày giờ tạo
        public int? UserSign { get; set; } // người tạo
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú
    }
}
