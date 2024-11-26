
namespace HNOne.Model.Models
{
    public class EnumCatagoryModel : AuditableModel
    {
        public Guid id { get; set; } 
        public string? enumType { get; set; } // Loại enum
        public string? enumTypeName { get; set; } // Loại enum
        public string? code { get; set; } // mã loại
        public string? name { get; set; } // tên loại
        public string? value { get; set; } // config nếu có
        public string? value1 { get; set; } // config nếu có 1
        public string? value2 { get; set; } // config nếu có 2
        public string? value3 { get; set; } // config nếu có 2
        public string? value4 { get; set; } // config nếu có 2
        public int rowOrder { get; set; }
    }
}
