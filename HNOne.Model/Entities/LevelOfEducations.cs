using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HNOne.Model.Entities
{
    [Table("LevelOfEducations")]
    public sealed class LevelOfEducations : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        public int EmployeeId { get; set; } // mã nhân viên
        public int? FromYear { get; set; }
        public int? ToYear { get; set; }
        [MaxLength(500)]
        public string? LevelOfEducation { get; set; } // trình độ đào tạo. Khai cáo trong Enum
        [MaxLength(500)]
        public string? EducationalInstitution1 { get; set; } // Nơi đào tạo. Khai cáo trong Enum
        [MaxLength(500)]
        public string? EducationalInstitution2 { get; set; } // Khoa. Khai cáo trong Enum
        [MaxLength(250)]
        public string? MajorCode { get; set; } // Chuyên ngành
        [MaxLength(50)]
        public string? RankingCode { get; set; } // Xếp loại 1. Khai báo trong enum
        [MaxLength(250)]
        public string? RankingName { get; set; } // Xếp loại 1. Khai báo trong enum
        public bool IsComplete { get; set; } // đã tố nghiệp ?
    }
}
