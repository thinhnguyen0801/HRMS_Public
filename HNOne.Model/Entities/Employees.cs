using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// bảng nhân viên
    /// </summary>
    [Table("Employees")]
    public class Employees : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(50)]
        public string? Code { get; set; }
        [MaxLength(250)]
        public string? Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? StatusId { get; set; } // Tình trạng
        [MaxLength(50)]
        public string? Gender { get; set; }
        [MaxLength(1000)]
        public string? PlaceOfBirth { get; set; } // Nơi sinh
        [MaxLength(1000)]
        public string? PlaceOfOrigin { get; set; } // Quê quán
        [MaxLength(100)]
        public string? Religion { get; set; } // Tôn giáo
        [MaxLength(100)]
        public string? Ethnicity { get; set; } // Dân tộc
        [MaxLength(1000)]
        public string? ImageUrl { get; set; } // Đường dẫn ảnh
        [MaxLength(100)]
        public string? MaritalStatus { get; set; } // Tình trạng hôn nhân
        public DateTime? StartDate { get; set; } // Ngày vào làm
        [MaxLength(1000)]
        public string? Remark { get; set; } // ghi chú
        [MaxLength(20)]
        public string? CIC { get; set; } // số căn cước
        public DateTime? IssuanceDateCIC{ get; set; } // Ngày cấp CCCD
        [MaxLength(1000)]
        public string? PlaceOfIssuanceCIC { get; set; } // nơi cấp CCCD
        [MaxLength(50)]
        public string? Phone1 { get; set; }
        [MaxLength(50)]
        public string? Phone2 { get; set; }
        [MaxLength(50)]
        public string? Phone3 { get; set; }
        [MaxLength(150)]
        public string? Email1 { get; set; }
        [MaxLength(150)]
        public string? Email2 { get; set; }
        [MaxLength(100)]
        public string? AccountNumber { get; set; } // Số tài khoản
        [MaxLength(100)]
        public string? BankName { get; set; } // tên ngân hàng
        [MaxLength(100)]
        public string? BankCode { get; set; } // mã chi nhánh ngân hàng
        [MaxLength(1000)]
        public string? BankBranch { get; set; } // chi nhánh ngân hàng
        [MaxLength(250)]
        public string? Beneficiary { get; set; } // người thụ hưởng
        [MaxLength(250)]
        public string? Nationality { get; set; } // Quốc tịch
        [MaxLength(50)]
        public string? TaxNumber { get; set; } // mã số thuế
        [MaxLength(50)]
        public string? PassportNumber { get; set; } // Hộ chiếu
        public int? LevelOfEducationId1 { get; set; } // trình độ học vấn 1
        public int? LevelOfEducationId2 { get; set; } // trình độ học vấn 2
        [MaxLength(250)]
        public string? MajorId1 { get; set; } // Chuyên ngành 1
        [MaxLength(250)]
        public string? MajorId2 { get; set; } // Chuyên ngành 2
        [MaxLength(500)]
        public string? EducationalInstitution1 { get; set; } // Trường đào tạo 1
        [MaxLength(500)]
        public string? EducationalInstitution2 { get; set; } // Trường đào tạo 2
        [MaxLength(250)]
        public string? Ranking1 { get; set; } // Xếp loại 1
        [MaxLength(250)]
        public string? Ranking2 { get; set; } // Xếp loại 2
        [MaxLength(250)]
        public string? LanguageLevel { get; set; } // Trình độ ngoại ngữ
        [MaxLength(250)]
        public string? RankingLang { get; set; } // Xếp loại ngoại ngữ
        [MaxLength(250)]
        public string? LevelOfComputerLiteracy { get; set; } // Trình độ tin học
        [MaxLength(250)]
        public string? RankingComputer { get; set; } // Xếp loại tin học
        [MaxLength(250)]
        public string? OtherSkills { get; set; } // Kĩ năng khác
        public DateTime? ProbationEndDate { get; set; } // ngày kết thúc thử việcs
        public int BranchId { get; set; } // ID chi nhánh
        public int DepartmentId { get; set; } // phòng ban
        public int PositionId { get; set; } // chức vụ
        public int? ManagerId { get; set; } // Người quản lý
        public int? AttendanceSheetId { get; set; } // ID bảng công
        
    }
}
