using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// bảng danh mục enum cấu hình
    /// </summary>
    [Table("EnumCatagories")]
    public class EnumCatagories
    {
        [Key]
        public Guid Id { get; set; } // sinh mã guid dưới sql
        [MaxLength(250)]
        public string? EnumType { get; set; } // Loại enum
        [MaxLength(50)]
        public string? Code { get; set; } // mã loại
        [MaxLength(250)]
        public string? Name { get; set; } // tên loại
        [MaxLength(250)]
        public string? Value { get; set; } // config nếu có
        [MaxLength(250)]
        public string? Value1 { get; set; } // config nếu có 1
        [MaxLength(250)]
        public string? Value2 { get; set; } // config nếu có 2
        public int? UserSign { get; set; }
        public DateTime? DateTracking { get; set; }
        public int RowOrder { get; set; }

    }
}
