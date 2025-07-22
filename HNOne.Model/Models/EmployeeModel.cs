using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public string? employeeType { get; set; } // loại nhân viên
        public DateTime? dateOfBirth { get; set; }
        public string? statusId { get; set; } // Tình trạng
        public string? gender { get; set; }
        public string? placeOfBirth { get; set; } // Nơi sinh
        public string? placeOfOrigin { get; set; } // Quê quán
        public string? religion { get; set; } // Tôn giáo
        public string? ethnicity { get; set; } // Dân tộc
        public string? imageUrl { get; set; } // Đường dẫn ảnh
        public string? imageViewUrl { get; set; } // Đường dẫn ảnh
        public string? nationality { get; set; } // Quốc tịch
        public string? maritalStatus { get; set; } // Tình trạng hôn nhân
        public string? maritalStatusName { get; set; } // Tình trạng hôn nhân
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
        public string? taxNumber { get; set; } // mã số thuế
        public string? accountNumber { get; set; } // Số tài khoản
        public string? bankName { get; set; } // tên ngân hàng
        public string? bankBranch { get; set; } // chi nhánh ngân hàng
        public string? beneficiary { get; set; } // người thụ hưởng
        public string? remark { get; set; } // ghi chú
        
        public int? managerId { get; set; } // Người quản lý trực tiếp
        public string? managerCode { get; set; } // Người quản lý
        public string? managerName { get; set; } // Người quản lý
        public int? managerId2 { get; set; } // Người quản lý gián tiếp
        public string? managerCode2 { get; set; } // Người quản lý
        public string? managerName2 { get; set; } // Người quản lý
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
        public string? subDepartmentCode { get; set; }
        public string? subDepartmentName { get; set; }
        public string? link { get; set; }
        public string? statusName { get; set; } // trạng thái làm việc
        public string? textColor { get; set; } // màu chữ
        #region thông tin cmnd, hộ chiếu
        public string? cIC { get; set; } // số căn cước
        public DateTime? issuanceDateCIC { get; set; } // Ngày cấp CCCD
        public string? placeOfIssuanceCIC { get; set; } // nơi cấp CCCD
        public DateTime? expiryDateCIC { get; set; } // ngày hết hạn CCCD
        public string? passportNumber { get; set; } // số hộ chiếu
        public DateTime? issueDatePassport { get; set; } // Ngày cấp hộ chiếu
        public string? placeOfIssuePassport { get; set; } // nơi cấp hộ chiếu
        public DateTime? expiryDatePassport { get; set; } // ngày hết hạn hộ chiếu
        #endregion

        #region Thông tin liên hệ
        public string? phone1 { get; set; } // ĐT di động
        public string? phone2 { get; set; } // ĐT cơ quan
        public string? phone3 { get; set; } // Đt nhà riêng
        public string? phone4 { get; set; } //Đt khác
        public string? email1 { get; set; } // Email cá nhân
        public string? email2 { get; set; } // Email cơ quan
        public string? email3 { get; set; } // Email khác
        public string? provinceCode { get; set; } // Tỉnh thành phố
        public string? provinceName { get; set; } // Tên Tỉnh thành phố
        #endregion

        #region Hộ khẩu thường trú
        public string? countryCode1 { get; set; } // Quốc gia
        public string? countryName1 { get; set; } // Tên quốc gia
        public string? provinceCode1 { get; set; } // Tỉnh thành phố
        public string? provinceName1 { get; set; } // Tên Tỉnh thành phố
        public string? districtCode1 { get; set; } //  quận huyện
        public string? districtName1 { get; set; } // Tên quận huyện
        public string? wardCode1 { get; set; } //  xã phường
        public string? wardName1 { get; set; } // Tên xã phường
        public string? houseNumber1 { get; set; } // số nhà
        public string? placeOfResidence { get; set; } // địa chỉ thường trú
        public string? householdRegistrationNumber { get; set; } // Số hộ khẩu
        public string? householdNumber { get; set; } // Số hộ gia đình

        #endregion

        #region Chổ ở hiện nay
        public string? countryCode2 { get; set; } // Quốc gia
        public string? countryName2 { get; set; } // Tên quốc gia
        public string? provinceCode2 { get; set; } // Tỉnh thành phố
        public string? provinceName2 { get; set; } // Tên Tỉnh thành phố
        public string? districtCode2 { get; set; } //  quận huyện
        public string? districtName2 { get; set; } // Tên quận huyện
        public string? wardCode2 { get; set; } //  xã phường
        public string? wardName2 { get; set; } // Tên xã phường
        public string? houseNumber2 { get; set; } // số nhà/ đường/ thôn xóm
        public string? temporaryAddress { get; set; } // địa chỉ tạm trú (Chỗ ở hiện tại)
        public bool isEqualsHousehold { get; set; } // Giống như hộ khẩu
        #endregion

        #region Liên hệ khẩn cấp
        public string? fullName1 { get; set; } // Tên nhân viên
        public string? relationship { get; set; }
        public string? relationshipName { get; set; }
        public string? phone5 { get; set; } // ĐT di động
        public string? phone6 { get; set; } // ĐT nhà riêng
        public string? email4 { get; set; } // Email khác
        public string? contactAddress { get; set; } // địa chỉ liên hệ
        #endregion

        #region Thông tin công việc
        public int? attendanceSheetId { get; set; } // id bảng công
        public string? attendanceSheetCode { get; set; } // mã chấm công
        public int branchId { get; set; } // ID chi nhánh
        public string? branchName { get; set; } //  chi nhánh
        public int workingBranchId { get; set; } // ID chi nhánh làm việc
        public string? workingBranchCode { get; set; } // ID chi nhánh làm việc
        public string? workingBranchName { get; set; } // ID chi nhánh làm việc
        public int departmentId { get; set; } // phòng ban
        public int positionId { get; set; } // chức vụ
        public int? titleId { get; set; } // chức danh
        public int? subDepartmentId { get; set; } // bộ phận
        public DateTime? traineeDate { get; set; } // Ngày tập sự
        public DateTime? probationStartDate { get; set; } // ngày thử việc
        public DateTime? probationEndDate { get; set; } // ngày kết thúc thử việcs
        public string? shiftCode { get; set; } // ca làm việc
        public string? shiftName { get; set; }
        public string? userName { get; set; } // tài khoản đăng nhập
        #endregion

    }

    /// <summary>
    /// Lịch sử lương nhân viên
    /// </summary>
    public class EmployeeSalaryHistoryModel
    {
        public int employeeId { get; set; }
        public string? employeeCode { get; set; }
        public string? employeeName { get; set; }
        public string? contractCode { get; set; }
        public DateTime? dateOfSigning { get; set; } // Ngày kí
        public int contractTypeId { get; set; } // Loại hợp đồng
        public string? contractTypeName { get; set; } // Loại hợp đồng
        public int branchId { get; set; } // ID chi nhánh
        public string? branchName { get; set; } //  chi nhánh
        public double salaryCoefficient { get; set; } // Hệ số lương
        public decimal totalSalary { get; set; } // Tổng lương
        public decimal netSalary { get; set; } // Tổng lương thực nhận
        public string? linkContract { get; set; }
        public string? employeeType { get; set; } // loại nhân viên
        public string? employeeTypeName { get; set; } // loại nhân viên
        public int employeeSignatureId { get; set; } // nhân viên kí
        public string? employeeSignatureCode { get; set; }
        public string? employeeSignatureName { get; set; }
        public string? contractAppendixCode { get; set; } // số phụ lục
        public int? contractNumber { get; set; } // Phụ lục số mấy rồi
        public string? linkContractAppendix { get; set; }
    }
}
