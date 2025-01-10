using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model.Entities
{

    /// <summary>
    /// Bảng lương từng nhân viên trong tháng
    /// </summary>
    [Table("Payrolls")]
    public sealed class Payrolls : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public long Id { get; set; }
        public int EmployeeId { get; set; } // id nhân viên
        [MaxLength(50)]
        [Required]
        public string? EmployeeCode { get; set; } // Mã nhân viên
        public int BranchId { get; set; } // id chi nhánh
        public int Month { get; set; } // tháng công
        public int Year { get; set; } // năm công
        public bool IsLocked { get; set; } // chốt chưa, chốt những ai
        #region Dữ liệu công của nhân viên
        public int AttendanceSummaryId { get; set; } // Id bảng công
        public double TNC { get; set; } // tổng ngày công
        public double CDM { get; set; } // công định mức của tháng
        public double GCDM { get; set; } // giờ công định mức của tháng
        public double CTT { get; set; } // công thực tế
        public double NL { get; set; } // những ngày nghỉ lễ
        public double NPN { get; set; } // nghỉ phép năm
        public double NCD { get; set; } // nghỉ chế độ
        public double NPKL { get; set; } // nghỉ phép không lương
        public double NB { get; set; } // nghỉ bù
        public double NKP { get; set; } // nghỉ không phép
        public double CTPC { get; set; } // số công tính phụ cấp
        public double TGDLTVS { get; set; } // thời gian đi trễ về sớm
        public double SLDLTVS { get; set; } // số lần đi trễ về sớm
        public double SGT { get; set; } // số giờ thiếu
        public double SGTC { get; set; } // số giờ trừ công
        public double GCTC { get; set; } // giờ công của 1 ngày
        public double TGTC { get; set; } // số giờ tăng ca
        public double SGTCTC { get; set; } // số giờ tăng ca tiêu chuẩn
        public double SGTCTT { get; set; } // số giờ tăng ca của tháng trước
        public double SGTCKT { get; set; } // số giờ tăng ca được chuyển sang tháng tiếp theo

        #endregion

        #region Thông tin hợp đồng & lương
        public int ContractId { get; set; } // Id hợp đồng
        [MaxLength(50)]
        [Required]
        public string? ContractCode { get; set; }
        public int ContractTypeId { get; set; } // Loại hợp đồng
        [MaxLength(250)]
        public string? ContractTypeName { get; set; } // Loại hợp đồng
        public bool IsCompanyDeduction { get; set; } // cty đóng trích nộp thay
        public bool IsCompanyInsurance { get; set; } // cty đóng trích nộp thay
        [Column(TypeName = "decimal(19, 6)")]
        public decimal BasicSalary { get; set; } // Lương cơ bản
        [Column(TypeName = "decimal(19, 6)")]
        public decimal NegotiatedSalary { get; set; } // Lương thỏa thuận
        public double SalaryCoefficient { get; set; } // Hệ số lương
        [Column(TypeName = "decimal(19, 6)")]
        public decimal ConvertedNegotiatedSalary { get; set; } // Lương thỏa thuận đã quy đổi với hệ số lương
        [Column(TypeName = "decimal(19, 6)")]
        public decimal AllowanceSalary { get; set; } // Lương phụ cấp
        [Column(TypeName = "decimal(19, 6)")]
        public decimal TotalSalaryGross { get; set; } // Tổng lương trước thuế
        [Column(TypeName = "decimal(19, 6)")]
        public decimal TotalSalaryNet { get; set; } // Tổng lương net

        [Column(TypeName = "decimal(19, 6)")]
        public decimal OvertimeSalary { get; set; } // lương số giờ tăng ca
        [Column(TypeName = "decimal(19, 6)")]
        public decimal ActualSalary { get; set; } // lương công thực tế
        [Column(TypeName = "decimal(19, 6)")]
        public decimal AnnualLeaveSalary { get; set; } // lương nghỉ phép năm
        [Column(TypeName = "decimal(19, 6)")]
        public decimal RegulatedSalary { get; set; } // lương nghỉ chế độ
        [Column(TypeName = "decimal(19, 6)")]
        public decimal HolidaySalary { get; set; } // lương nghỉ lễ
        [Column(TypeName = "decimal(19, 6)")]
        public decimal MissingWorkingHourSalary { get; set; } // lương số giờ thiếu
        [Column(TypeName = "decimal(19, 6)")]
        public decimal LateSalary { get; set; } // lương đi trễ về sớm
        #endregion

        #region Thông tin về trích nộp
        [Column(TypeName = "decimal(19, 6)")]
        public decimal ContributionSalarySI { get; set; } // mức đóng BHXH
        [Column(TypeName = "decimal(19, 6)")]
        public decimal DeductionEnterpriseSI { get; set; } // trích nộp bảo hiểm xã hội công ty (SI social insurance)
        [Column(TypeName = "decimal(19, 6)")]
        public decimal DeductionEmployeeSI { get; set; } // trích nộp bảo hiểm xã hội nhân viên (SI social insurance)
        [Column(TypeName = "decimal(19, 6)")]
        public decimal ContributionSalaryHI { get; set; } // mức đóng bảo hiểm y tế
        [Column(TypeName = "decimal(19, 6)")]
        public decimal DeductionEnterpriseHI { get; set; } // trích nộp bảo hiểm y tế công ty (HI health insurance)
        [Column(TypeName = "decimal(19, 6)")]
        public decimal DeductionEmployeeHI { get; set; } // trích nộp bảo hiểm y tế nhân viên (HI health insurance) 
        [Column(TypeName = "decimal(19, 6)")]
        public decimal ContributionSalaryUI { get; set; } // mức đóng bảo hiểm thất nghiệp
        [Column(TypeName = "decimal(19, 6)")]
        public decimal DeductionEnterpriseUI { get; set; } // trích nộp bảo hiểm thất nghiệp công ty (UI unemployment insurance)
        [Column(TypeName = "decimal(19, 6)")]
        public decimal DeductionEmployeeUI { get; set; } // trích nộp bảo hiểm thất nghiệp nhân viên (UI unemployment insurance) 
        [Column(TypeName = "decimal(19, 6)")]
        public decimal ContributionSalaryAI { get; set; } // mức đóng bảo hiểm tai nạn
        [Column(TypeName = "decimal(19, 6)")]
        public decimal DeductionEnterpriseAI { get; set; } // trích nộp bảo hiểm tai nạn công ty (AI accident insurance)
        [Column(TypeName = "decimal(19, 6)")]
        public decimal DeductionEmployeeAI { get; set; } // trích nộp bảo hiểm tai nạn nhân viên (AI accident insurance) 
        [Column(TypeName = "decimal(19, 6)")]
        public decimal TotalDeductionEnterprise { get; set; } // tổng trích nộp công ty
        [Column(TypeName = "decimal(19, 6)")]
        public decimal TotalDeductionEmployee { get; set; } // tổng trích nộp công ty
        [Column(TypeName = "decimal(19, 6)")]
        public decimal TotalDeduction { get; set; } // tổng trích nộp
        [Column(TypeName = "decimal(19, 6)")]
        public decimal UnionFeeSalary { get; set; } // lương đóng phí công đoàn
        [Column(TypeName = "decimal(19, 6)")]
        public decimal DeductionEnterpriseUF { get; set; } // trích nộp công ty Phí công đoàn (UF Union fees)
        [Column(TypeName = "decimal(19, 6)")]
        public decimal DeductionEmployeeUF { get; set; } // trích nộp nhân viên Phí công đoàn (UF Union fees) 
        #endregion

        #region Thuế thu nhập cá nhân
        public int SalaryParameterId { get; set; } // Id mức thuế
        [MaxLength(50)]
        public string? TaxTypeCode { get; set; } // loại tính thuế
        [MaxLength(250)]
        public string? TaxTypeName { get; set; } // tên loại tính thuế TNCN
        [Column(TypeName = "decimal(19, 6)")]
        public int TaxtRateId { get; set; } // Id mức thuế
        [Column(TypeName = "decimal(19, 6)")]
        public int TaxBracket { get; set; } // bậc thuế
        [Column(TypeName = "decimal(19, 6)")]
        public decimal MinTaxSalary { get; set; } // giời hạn tính thuế
        [Column(TypeName = "decimal(19, 6)")]
        public decimal MaxTaxSalary { get; set; } // giời hạn tính thuế
        [Column(TypeName = "decimal(19, 6)")]
        public double TaxRate { get; set; } // % thuế
        [Column(TypeName = "decimal(19, 6)")]
        public decimal ProgressiveAmount { get; set; } // số tiền lũy tiến
        [Column(TypeName = "decimal(19, 6)")]
        public decimal StandardTax { get; set; } // Thuế giảm trừ bản thân
        [Column(TypeName = "decimal(19, 6)")]
        public decimal FamilyCircumstanceTaxDeduction { get; set; } // Giảm trừ gia cảnh
        [Column(TypeName = "decimal(19, 6)")]
        public int NumOfPeopleTaxFCTaxDeduction { get; set; } // số người giảm trừ
        [Column(TypeName = "decimal(19, 6)")]
        public decimal TotalFCTaxDeduction { get; set; } // tổng tiền giảm trừ gia cảnh
        [Column(TypeName = "decimal(19, 6)")]
        public decimal TaxableIncome { get; set; } // Thu nhập tính thuế
        [Column(TypeName = "decimal(19, 6)")]
        public decimal TaxAllowance { get; set; } // phụ cấp tính thuế
        [Column(TypeName = "decimal(19, 6)")]
        public decimal TaxPayment { get; set; } // Số tiền đóng thuế
        #endregion
    }
}
