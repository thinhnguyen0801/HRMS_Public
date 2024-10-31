using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    ///  bảng quan hệ gia đình
    /// </summary>
    [Table("FamilyRelationships")]
    public class FamilyRelationships : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Không tự tăng
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        [MaxLength(250)]
        public string? Name { get; set; }
        [MaxLength(50)]
        public string? RelationshipId { get; set; } // mã loại quan hệ -> lấy từ enum
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(1000)]
        public string? PlaceOfBirth { get; set; } // Nơi sinh
        [MaxLength(250)]
        public string? Occupation { get; set; } // Nghề nghiệp
        [MaxLength(1000)]
        public string? PlaceOfOrigin { get; set; } // Quê quán
        [MaxLength(1000)]
        public string? TemporaryAddress { get; set; } // địa chỉ tạm trú
        [MaxLength(1000)]
        public string? ContactAddress { get; set; } // địa chỉ liên hệ
        [MaxLength(50)]
        public string? PhoneNumber { get; set; }
        [MaxLength(20)]
        public string? CIC { get; set; } // số căn cước
        public DateTime? IssuanceDateCIC { get; set; } // Ngày cấp CCCD
        [MaxLength(1000)]
        public string? Remark { get; set; } // ghi chú
    }
}
