using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Bảng lưu log dữ liệu khi cập nhật thông tin
    /// </summary>
    [Table("AuditLogs")]
    public sealed class AuditLogs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public int Id { get; set; }
        public int DocEntry { get; set; }
        public int UserId { get; set; }
        public string? EntityType { get; set; }
        public string? Action { get; set; }
        public DateTime TimeStamp { get; set; }
        public string? JsonPropsChange { get; set; }
    }
}
