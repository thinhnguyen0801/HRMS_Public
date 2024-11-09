using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// bảng sự kiện của các controller
    /// </summary>
    [Table("EventConfigurations")]
    public sealed class EventConfigurations
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(100)]
        public string? ActionKey { get; set; } // CONTROLLER_ACTION
        [MaxLength(100)]
        public string? ActionName { get; set; } // Thêm xóa sửa
        [MaxLength(50)]
        public string? MenuID { get; set; } // menu
        [MaxLength(250)]
        public string? MenuName { get; set; } // Tên menu để hiển thị trên phân quyền
        [MaxLength(50)]
        public string? ParentID { get; set; } // Parent
        [MaxLength(250)]
        public string? ParentName { get; set; } // Parent
        [MaxLength(50)]
        public string? EventType { get; set; } // loại sự kiện
        public int RowOrder { get; set; } // sắp xếp
    }
}
