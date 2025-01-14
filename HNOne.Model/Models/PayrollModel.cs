using System.Text.RegularExpressions;

namespace HNOne.Model.Models
{
    public class PayrollModel
    {
        public int status { get; set; }
        public string? message { get; set; }
        public int rowOrder { get; set; } // số thứ tự
        public int employeeId { get; set; }
        public string? employeeCode { get; set; }
        public string? employeeName { get; set; }
        public int branchId { get; set; }
        public string? branchCode { get; set; }
        public string? branchName { get; set; }
        public int departmentId { get; set; } // phòng ban
        public string? departmentName { get; set; }
        public int titleId { get; set; } // chức danh
        public string? titleCode { get; set; } // chức danh
        public string? titleName { get; set; } //
        public string? shiftCode { get; set; } // ca làm việc mặc định của nhân viên
        public int month { get; set; }
        public int year { get; set; }
        public bool isLocked { get; set; }

        #region Thông tin hợp đồng & lương
        public int contractId { get; set; } // Id hợp đồng
        public string? contractCode { get; set; }
        public int contractTypeId { get; set; } // Loại hợp đồng
        public int contractAppendixId { get; set; } // Id phụ lục hợp đồng
        public string? contractAppendixCode { get; set; } // Mã phụ lục hợp đồng
        public string? contractTypeName { get; set; } // Loại hợp đồng
        public bool isCompanyDeduction { get; set; } // cty đóng trích nộp thay
        public bool isCompanyInsurance { get; set; } // cty đóng trích nộp thay
        public decimal basicSalary { get; set; } // Lương cơ bản
        public decimal negotiatedSalary { get; set; } // Lương thỏa thuận
        public double salaryCoefficient { get; set; } // Hệ số lương
        public decimal convertedNegotiatedSalary { get; set; } // Lương thỏa thuận đã quy đổi với hệ số lương
        public decimal allowanceSalary { get; set; } // Lương phụ cấp
        public decimal totalSalaryGross { get; set; } // Tổng lương trước thuế
        public decimal totalSalaryNet { get; set; } // Tổng lương trước thuế
        #endregion


        #region Thuế thu nhập cá nhân
        public int salaryParameterId { get; set; } // Id mức thuế
        public string? taxTypeCode { get; set; } // loại tính thuế
        public string? taxTypeName { get; set; } // tên loại tính thuế TNCN
        public int taxtRateId { get; set; } // Id mức thuế
        public int taxBracket { get; set; } // bậc thuế
        public decimal minTaxSalary { get; set; }
        public decimal maxTaxSalary { get; set; }
        public double taxRate { get; set; } // % thuế
        public decimal progressiveAmount { get; set; } // số tiền lũy tiến
        public decimal standardTax { get; set; } // Thuế giảm trừ bản thân
        public decimal familyCircumstanceTaxDeduction { get; set; } // Giảm trừ gia cảnh
        public int numOfPeopleTaxFCTaxDeduction { get; set; } // số người giảm trừ
        public decimal totalFCTaxDeduction { get; set; } // tổng tiền giảm trừ gia cảnh
        public decimal taxableIncome { get; set; } // Thu nhập tính thuế
        public decimal taxAllowance { get; set; } // phụ cấp tính thuế
        public decimal taxPayment { get; set; } // Số tiền đóng thuế

        #endregion

        #region Thông tin về trích nộp
        public decimal contributionSalarySI { get; set; } // mức đóng BHXH
        public decimal deductionEnterpriseSI { get; set; } // trích nộp bảo hiểm xã hội công ty (SI social insurance)
        public decimal deductionEmployeeSI { get; set; } // trích nộp bảo hiểm xã hội nhân viên (SI social insurance)

        public decimal contributionSalaryHI { get; set; } // mức đóng bảo hiểm y tế
        public decimal deductionEnterpriseHI { get; set; } // trích nộp bảo hiểm y tế công ty (HI health insurance)
        public decimal deductionEmployeeHI { get; set; } // trích nộp bảo hiểm y tế nhân viên (HI health insurance) 

        public decimal contributionSalaryUI { get; set; } // mức đóng bảo hiểm thất nghiệp
        public decimal deductionEnterpriseUI { get; set; } // trích nộp bảo hiểm thất nghiệp công ty (UI unemployment insurance)
        public decimal deductionEmployeeUI { get; set; } // trích nộp bảo hiểm thất nghiệp nhân viên (UI unemployment insurance) 

        public decimal contributionSalaryAI { get; set; } // mức đóng bảo hiểm tai nạn
        public decimal deductionEnterpriseAI { get; set; } // trích nộp bảo hiểm tai nạn công ty (AI accident insurance)
        public decimal deductionEmployeeAI { get; set; } // trích nộp bảo hiểm tai nạn nhân viên (AI accident insurance) 

        public decimal totalDeductionEnterprise { get; set; } // tổng trích nộp công ty
        public decimal totalDeductionEmployee { get; set; } // tổng trích nộp công ty
        public decimal totalDeduction { get; set; } // tổng trích nộp

        public decimal unionFeeSalary { get; set; } // lương đóng phí công đoàn
        public decimal deductionEnterpriseUF { get; set; } // trích nộp công ty Phí công đoàn (UF Union fees)
        public decimal deductionEmployeeUF { get; set; } // trích nộp nhân viên Phí công đoàn (UF Union fees) 
        #endregion

        #region Thông tin về công
        public int attendanceSummaryId { get; set; }
        public double tNC { get; set; } // tổng ngày công
        public double cDM { get; set; } // công định mức của tháng
        public double gCDM { get; set; } // giờ công định mức của tháng
        public double cTT { get; set; } // công thực tế
        public double nPN { get; set; } // nghỉ phép năm
        public double nL { get; set; } // nghỉ lễ
        public double nCD { get; set; } // nghỉ chết độ
        public double nPKL { get; set; } // nghỉ phép không lương
        public double nB { get; set; } // nghỉ bù
        public double nKP { get; set; } // nghỉ không phép
        public double cTPC { get; set; } // số công tính phụ cấp
        public double tGDLTVS { get; set; } // thời gian đi trễ về sớm
        public double sLDLTVS { get; set; } // số lần đi trễ về sớm
        public double sGT { get; set; } // số giờ thiếu
        public double sGTC { get; set; } // số giờ trừ công
        public double gCTC { get; set; } // giờ công của 1 ngày
        public double tGTC { get; set; } // số giờ tăng ca
        public double sGTCTC { get; set; } // số giờ tăng ca tiêu chuẩn
        public double sGTCTT { get; set; } // số giờ tăng ca của tháng trước
        public double sGTCKT { get; set; } // số giờ tăng ca được chuyển sang tháng tiếp theo
        public decimal overtimeSalary { get; set; } // lương số giờ tăng ca
        public decimal actualSalary { get; set; } // lương công thực tế
        public decimal annualLeaveSalary { get; set; } // lương nghỉ phép năm
        public decimal regulatedSalary { get; set; } // lương nghỉ chế độ
        public decimal holidaySalary { get; set; } // lương nghỉ lễ
        public decimal missingWorkingHourSalary { get; set; } // lương số giờ thiếu
        public decimal lateSalary { get; set; } // lương đi trễ về sớm
        #endregion

    }
}
