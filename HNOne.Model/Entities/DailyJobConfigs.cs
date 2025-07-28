using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Bảng lưu các job chạy định kỳ
    /// </summary>
    [Table("DailyJobConfigs")]
    public sealed class DailyJobConfigs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public int Id { get; set; }
        public int DocEntry { get; set; }
        [MaxLength(250)]
        public string? ObjType { get; set; } // loại chứng từ
        public DateTime ExecuteDate{ get; set; } // ngày giờ chạy
        public string? SQLText { get; set; } // câu sql
        public bool IsCompleted { get; set; } // hoàn thành chứa
        public DateTime? CompletedDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? UserSign { get; set; }
    }
}
