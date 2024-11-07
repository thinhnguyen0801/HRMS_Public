
namespace HNOne.Model.Models
{
    public class FamilyRelationshipModel : AuditableModel
    {
        public int id { get; set; }
        public int employeeId { get; set; }
        public string? name { get; set; }
        public string? relationshipId { get; set; } // mã loại quan hệ -> lấy từ enum
        public string? relationshipName { get; set; } // Tên mối quan hệ
        public DateTime? dateOfBirth { get; set; }
        public string? placeOfBirth { get; set; } // Nơi sinh
        public string? occupation { get; set; } // Nghề nghiệp
        public string? placeOfOrigin { get; set; } // Quê quán
        public string? temporaryAddress { get; set; } // địa chỉ tạm trú
        public string? contactAddress { get; set; } // địa chỉ liên hệ
        public string? phoneNumber { get; set; }
        public string? cIC { get; set; } // số căn cước
        public DateTime? issuanceDateCIC { get; set; } // Ngày cấp CCCD
        public string? remark { get; set; } // ghi chú
    }
}
