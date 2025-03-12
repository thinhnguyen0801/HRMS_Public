using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Danh mục ngày nghỉ lễ
    /// </summary>
    [Table("HolidayCatagories")]
    public sealed class HolidayCatagories : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public int Id { get; set; }
        [MaxLength(250)]
        [Required]
        public string? Name { get; set; } // tên loại ngày nghỉ
        public int BranchId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        [MaxLength(250)]
        public string? Color { get; set; }
        [MaxLength(50)]
        public string? Type { get; set; } // loại ngày nghỉ lấy ở enum
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú
    }
}
