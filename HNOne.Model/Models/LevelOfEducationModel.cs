
namespace HNOne.Model.Models
{
    public class LevelOfEducationModel : AuditableModel
    {
        public int id { get; set; }
        public int employeeId { get; set; } // mã nhân viên
        public int? fromYear { get; set; }
        public int? toYear { get; set; }
        public string? levelOfEducation { get; set; } // trình độ đào tạo. Khai cáo trong Enum
        public string? educationalInstitution1 { get; set; } // Nơi đào tạo. Khai cáo trong Enum
        public string? educationalInstitution2 { get; set; } // Khoa. Khai cáo trong Enum
        public string? majorCode { get; set; } // Chuyên ngành
        public string? rankingCode { get; set; } // Xếp loại 1. Khai báo trong enum
        public string? rankingName { get; set; } // Xếp loại 1. Khai báo trong enum
        public bool isComplete { get; set; } // đã tố nghiệp ?
    }
}
