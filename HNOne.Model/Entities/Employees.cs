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
        #region thông tin chung

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; } // ID nhân viên
        [MaxLength(50)]
        public string? Code { get; set; } // Mã nhân viên
        [MaxLength(250)]
        public string? Name { get; set; } // Tên nhân viên
        [MaxLength(50)]
        public string? EmployeeType { get; set; } // Loại nhân viên
        public DateTime? DateOfBirth { get; set; } // Ngày sinh (Khi check chỉ lấy năm sinh. default 1/1)
        public bool IsOnlybirthYear { get; set; } // Chỉ lấy năm sinh. đặt ngày chỗ ngày sinh
        
        [MaxLength(50)]
        public string? Gender { get; set; } // Giới tính
        [MaxLength(50)]
        public string? TaxNumber { get; set; } // mã số thuế
        public int DepartmentId { get; set; } // phòng ban làm việc
        [MaxLength(100)]
        public string? Ethnicity { get; set; } // Dân tộc
        [MaxLength(100)]
        public string? Religion { get; set; } // Tôn giáo
        [MaxLength(250)]
        public string? Nationality { get; set; } // Quốc tịch
        [MaxLength(100)]
        public string? MaritalStatus { get; set; } // Tình trạng hôn nhân khai trong enum
        [MaxLength(1000)]
        public string? ImageUrl { get; set; } // Đường dẫn ảnh
        public int BranchId { get; set; } // ID chi nhánh
        public int WorkingBranchId { get; set; } // ID chi nhánh làm việc
        #endregion

        #region thông tin cmnd, hộ chiếu

        [MaxLength(20)]
        public string? CIC { get; set; } // số căn cước
        public DateTime? IssuanceDateCIC { get; set; } // Ngày cấp CCCD
        [MaxLength(1000)]
        public string? PlaceOfIssuanceCIC { get; set; } // nơi cấp CCCD
        public DateTime? ExpiryDateCIC { get; set; } // ngày hết hạn CCCD
        [MaxLength(50)]
        public string? PassportNumber { get; set; } // số hộ chiếu
        public DateTime? IssueDatePassport { get; set; } // Ngày cấp hộ chiếu
        [MaxLength(1000)]
        public string? PlaceOfIssuePassport { get; set; } // nơi cấp hộ chiếu
        public DateTime? ExpiryDatePassport { get; set; } // ngày hết hạn hộ chiếu

        #endregion

        #region trình độ bằng cấp
        [MaxLength(500)]
        public string? LevelOfEducationId1 { get; set; } // trình độ văn hóa. Khai cáo trong Enum
        [MaxLength(500)]
        public string? LevelOfEducationId2 { get; set; } // trình độ đào tạo. Khai cáo trong Enum
        [MaxLength(500)]
        public string? EducationalInstitution1 { get; set; } // Nơi đào tạo. Khai cáo trong Enum
        [MaxLength(500)]
        public string? EducationalInstitution2 { get; set; } // Khoa. Khai cáo trong Enum
        [MaxLength(250)]
        public string? MajorId1 { get; set; } // Chuyên ngành 1
        [MaxLength(250)]
        public string? MajorId2 { get; set; } // Chuyên ngành 2
        [MaxLength(250)]
        public string? Ranking1 { get; set; } // Xếp loại 1. Khai báo trong enum
        [MaxLength(250)]
        public string? Ranking2 { get; set; } // Xếp loại 2 Khai báo trong enum
        public int? GraduationYear { get; set; } // năm tốt nghiệp

        [MaxLength(250)]
        public string? LanguageLevel { get; set; } // Trình độ ngoại ngữ
        [MaxLength(250)]
        public string? LevelOfComputerLiteracy { get; set; } // Trình độ tin học
        [MaxLength(250)]
        public string? OtherSkills { get; set; } // Kĩ năng khác
        #endregion

        #region Thông tin liên hệ

        [MaxLength(50)]
        public string? Phone1 { get; set; } // ĐT di động
        [MaxLength(50)]
        public string? Phone2 { get; set; } // ĐT cơ quan
        [MaxLength(50)]
        public string? Phone3 { get; set; } // Đt nhà riêng
        [MaxLength(50)]
        public string? Phone4 { get; set; } //Đt khác
        [MaxLength(150)]
        public string? Email1 { get; set; } // Email cá nhân
        [MaxLength(150)]
        public string? Email2 { get; set; } // Email cơ quan
        [MaxLength(150)]
        public string? Email3 { get; set; } // Email khác
        [MaxLength(1000)]
        public string? PlaceOfOrigin { get; set; } // Nguyên quán
        [MaxLength(1000)]
        public string? PlaceOfBirth { get; set; } // Nơi sinh
        [MaxLength(50)]
        public string? ProvinceCode { get; set; } // Tỉnh thành phố
        [MaxLength(250)]
        public string? ProvinceName { get; set; } // Tên Tỉnh thành phố

        #endregion

        #region Hộ khẩu thường trú

        [MaxLength(50)]
        public string? CountryCode1 { get; set; } // Quốc gia
        [MaxLength(250)]
        public string? CountryName1 { get; set; } // Tên quốc gia
        [MaxLength(50)]
        public string? ProvinceCode1 { get; set; } // Tỉnh thành phố
        [MaxLength(250)]
        public string? ProvinceName1 { get; set; } // Tên Tỉnh thành phố
        [MaxLength(50)]
        public string? DistrictCode1 { get; set; } //  quận huyện
        [MaxLength(250)]
        public string? DistrictName1 { get; set; } // Tên quận huyện
        [MaxLength(50)]
        public string? WardCode1 { get; set; } //  xã phường
        [MaxLength(250)]
        public string? WardName1 { get; set; } // Tên xã phường
        [MaxLength(250)]
        public string? HouseNumber1 { get; set; } // số nhà
        [MaxLength(2500)]
        public string? PlaceOfResidence { get; set; } // địa chỉ thường trú
        [MaxLength(250)]
        public string? HouseholdRegistrationNumber { get; set; } // Số hộ khẩu
        [MaxLength(250)]
        public string? HouseholdNumber { get; set; } // Số hộ gia đình

        #endregion

        #region Chổ ở hiện nay
        [MaxLength(50)]
        public string? CountryCode2 { get; set; } // Quốc gia
        [MaxLength(250)]
        public string? CountryName2 { get; set; } // Tên quốc gia
        [MaxLength(50)]
        public string? ProvinceCode2 { get; set; } // Tỉnh thành phố
        [MaxLength(250)]
        public string? ProvinceName2 { get; set; } // Tên Tỉnh thành phố
        [MaxLength(50)]
        public string? DistrictCode2 { get; set; } //  quận huyện
        [MaxLength(250)]
        public string? DistrictName2 { get; set; } // Tên quận huyện
        [MaxLength(50)]
        public string? WardCode2 { get; set; } //  xã phường
        [MaxLength(250)]
        public string? WardName2 { get; set; } // Tên xã phường
        [MaxLength(250)]
        public string? HouseNumber2 { get; set; } // số nhà/ đường/ thôn xóm
        [MaxLength(2500)]
        public string? TemporaryAddress { get; set; } // địa chỉ tạm trú (Chỗ ở hiện tại)
        #endregion

        #region Liên hệ khẩn cấp
        [MaxLength(250)]
        public string? FullName1 { get; set; } // Tên nhân viên
        [MaxLength(250)]
        public string? Relationship { get; set; }
        [MaxLength(50)]
        public string? Phone5 { get; set; } // ĐT di động
        [MaxLength(50)]
        public string? Phone6 { get; set; } // ĐT nhà riêng
        [MaxLength(150)]
        public string? Email4 { get; set; } // Email khác
        [MaxLength(2500)]
        public string? ContactAddress { get; set; } // địa chỉ liên hệ
        #endregion

        #region Thông tin công việc
        public int? AttendanceSheetId { get; set; } // ID bảng công
        [MaxLength(250)]
        public string? AttendanceSheetCode { get; set; } // ID bảng công
        public int PositionId { get; set; } // chức vụ/vị trí
        public int? TitleId { get; set; } // chức danh
        [MaxLength(50)]
        public string? LevelCode { get; set; } // Cấp
        [MaxLength(50)]
        public string? GradeCode { get; set; } // Bậc
        [MaxLength(50)]
        public string? StatusId { get; set; } // Tình trạng. Lấy ở bảng Enum

        // quản lý trực tiếp, gián tiếp để sau, địa chỉ làm việc
        public DateTime? DateOfJoining { get; set; } // Ngày công ty
        public DateTime? TraineeDate { get; set; } // Ngày tập sự
        public DateTime? StartDate { get; set; } // Ngày vào làm
        public DateTime? ProbationStartDate { get; set; } // ngày thử việc
        public DateTime? ProbationEndDate { get; set; } // ngày kết thúc thử việcs

        [MaxLength(1000)]
        public string? Remark { get; set; } // ghi chú
        [MaxLength(50)]
        public string? ShiftCode { get; set; } // ca làm việc lấy từ bảng enum
        #endregion

        // Đơn vị công tác bên bảng WorkHistories
        // Thông tin bảo hiêm Insurances


        [MaxLength(100)]
        public string? AccountNumber { get; set; } // Số tài khoản
        [MaxLength(100)]
        public string? BankName { get; set; } // tên ngân hàng
        [MaxLength(1000)]
        public string? BankBranch { get; set; } // chi nhánh ngân hàng
        [MaxLength(250)]
        public string? Beneficiary { get; set; } // người thụ hưởng
        public int? ManagerId { get; set; } // Người quản lý trực tiếp
        public int? ManagerId2 { get; set; } // Người quản lý gián tiếp
        
        
    }
}
