using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Lịch sử công tác
    /// </summary>
    [Table("WorkHistories")]
    public class WorkHistories : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        public int EmployeeId { get; set; } // mã nhân viên
        [MaxLength(250)]
        public string? BranchName { get; set; } // Tên công ty
        [MaxLength(250)]
        public string? Position { get; set; } // Vị trí
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [MaxLength(250)]
        public string? Remark { get; set; } // Ghi chú
    }
}
