using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model.Models
{
    public class EmployeeModel
    {
        public int id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public DateTime? dateOfBirth { get; set; }
        public int? statusId { get; set; } // Tình trạng
        public string? gender { get; set; }
        public string? cIC { get; set; } // số căn cước
        public DateTime? issuanceDateCIC { get; set; } // Ngày cấp CCCD
        public string? placeOfIssuanceCIC { get; set; } // nơi cấp CCCD
        public string? placeOfBirth { get; set; } // Nơi sinh
        public string? placeOfOrigin { get; set; } // Quê quán
        public string? temporaryAddress { get; set; } // địa chỉ tạm trú
        public string? contactAddress { get; set; } // địa chỉ liên hệ
        public string? religion { get; set; } // Tôn giáo
        public string? ethnicity { get; set; } // Dân tộc
        public string? imageUrl { get; set; } // Đường dẫn ảnh
        public string? nationality { get; set; } // Quốc tịch
        public string? maritalStatus { get; set; } // Tình trạng hôn nhân
        public string? email1 { get; set; }
        public string? email2 { get; set; }
        public string? phone1 { get; set; }
        public string? phone2 { get; set; }
        public string? phone3 { get; set; }
        public string? educationalInstitution1 { get; set; } // Trường đào tạo 1
        public string? educationalInstitution2 { get; set; } // Trường đào tạo 2
        public string? majorId1 { get; set; } // Chuyên ngành 1
        public string? majorId2 { get; set; } // Chuyên ngành 2
        public string? ranking1 { get; set; } // Xếp loại 1
        public string? ranking2 { get; set; } // Xếp loại 2
        public string? languageLevel { get; set; } // Trình độ ngoại ngữ
        public string? levelOfComputerLiteracy { get; set; } // Trình độ tin học
        public string? otherSkills { get; set; } // Kĩ năng khác
        public DateTime? dateOfJoining { get; set; } // Ngày công ty
        public DateTime? startDate { get; set; } // Ngày vào làm
        public DateTime? probationEndDate { get; set; } // ngày kết thúc thử việcs
        public string? taxNumber { get; set; } // mã số thuế
        public string? accountNumber { get; set; } // Số tài khoản
        public string? bankName { get; set; } // tên ngân hàng
        public string? bankBranch { get; set; } // chi nhánh ngân hàng
        public string? beneficiary { get; set; } // người thụ hưởng
        public string? remark { get; set; } // ghi chú
        public int branchId { get; set; } // ID chi nhánh
        public int departmentId { get; set; } // phòng ban
        public int positionId { get; set; } // chức vụ
        public int? titleId { get; set; } // chức danh
        public int? managerId { get; set; } // Người quản lý
        public int? userSign { get; set; }
        public DateTime? updateDate { get; set; }
        public int? userSign2 { get; set; }
        public bool isDelete { get; set; }
        public string? deleteReason { get; set; }
        public DateTime? dateTracking { get; set; }
        public string? userSignName { get; set; }
        public string? userSign2Name { get; set; }
        public string? departmentCode { get; set; }
        public string? departmentName { get; set; }
        public string? positionCode { get; set; }
        public string? positionName { get; set; }
        public string? titleCode { get; set; }
        public string? titleName { get; set; }
        public string? link { get; set; }
        public string? statusName { get; set; } // trạng thái làm việc

    }
}
