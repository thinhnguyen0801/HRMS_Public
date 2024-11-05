

using System.ComponentModel.DataAnnotations;

namespace HNOne.Model.Models
{
    public class FamilyRelationshipModel
    {
        public int id { get; set; }
        public int employeeId { get; set; }
        public string? name { get; set; }
        public string? relationshipId { get; set; } // mã loại quan hệ -> lấy từ enum
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
        public DateTime? createDate { get; set; }
        public int? userSign { get; set; }
        public DateTime? updateDate { get; set; }
        public int? userSign2 { get; set; }
        public bool isDelete { get; set; }
        public string? deleteReason { get; set; }
        public DateTime? dateTracking { get; set; }
        public string? userSignName { get; set; }
        public string? userSign2Name { get; set; }
    }
}
