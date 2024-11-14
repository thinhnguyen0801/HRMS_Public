
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// bảng thông tin phê duyệt
    /// </summary>
    [Table("Approvals")]
    public sealed class Approvals
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public int Id { get; set; }
        public int BranchId { get; set; }
        public int DocEntry { get; set; }
        [MaxLength(150)]
        [Required]
        public string? ObjType { get; set; } // loại chứng từ
        [MaxLength(50)]
        [Required]
        public string? StatusCode { get; set; } // trạng thái phê duyệt
        public DateTime? CreateDate { get; set; } // ngày giờ tạo
        public int? UserSign { get; set; } // người tạo
        public DateTime? UpdateDate { get; set; } // ngày giờ cập nhật
        public int? UserSign2 { get; set; } // người cập nhật
        public DateTime? DateTracking { get; set; }
        public int EmployeeSignatureId { get; set; } // nhân viên kí
        [MaxLength(250)]
        public string? ApprovalRemark { get; set; } // ghi chú phê duyệt
    }
}
