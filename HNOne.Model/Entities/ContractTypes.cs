using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    [Table("ContractTypes")]
    public class ContractTypes : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(50)]
        public string? Code { get; set; } // mã
        [MaxLength(250)]
        public string? Name { get; set; }
        [MaxLength(500)]
        public string? Remark { get; set; }
        public int BranchId { get; set; }
        public string? StatusCode { get; set; } //trạng thái nhân viên
        public int Duration { get; set; } // thời hạn
        public int IndefiniteDuration { get; set; } // thời hạn không xác định
        public int NumberOfDaysReduced { get; set; } // số ngày cho phép giảm
        public bool IsActive { get; set; }
    }
}
