using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// bảng danh mục enum cấu hình
    /// </summary>
    [Table("EnumCatagories")]
    public class EnumCatagories : Auditable
    {
        [Key]
        public Guid Id { get; set; } // sinh mã guid dưới sql
        [MaxLength(250)]
        public string? EnumType { get; set; } // Loại enum
        [MaxLength(250)]
        public string? EnumTypeName { get; set; } // Tên Loại enum
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
        [MaxLength(250)]
        public string? Value3 { get; set; } // config nếu có 3
        [MaxLength(250)]
        public string? Value4 { get; set; } // config nếu có 4
        public int RowOrder { get; set; } // sắp xếp
        public bool IsAllowEditing { get; set; } // cho phép chỉnh sửa
        public bool IsAllowInsert { get; set; } // cho phép thêm mới không

    }
}
