
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Bảng phụ lục hợp đồng
    /// </summary>
    [Table("ContractAppendices")]
    public sealed class ContractAppendices
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        public int ContractId { get; set; } // hợp đồng
        public int EmployeeId { get; set; }
        public int BranchId { get; set; }
        public int TimesheetId { get; set; } // ID bảng công
        [MaxLength(250)]
        public string? ContractCode { get; set; } // số hợp đồng
        public DateTime? DateOfSigning { get; set; } // Ngày kí
        public DateTime? EffectiveDate { get; set; } // Ngày áp dụng phụ lục
        public DateTime? DeductionDate { get; set; } // Ngày trích nộp
        public int EmployeeSignatureId { get; set; } // nhân viên kí
        public int DepartmentId { get; set; } // chức vụ
        public int PositionId { get; set; } // chức vụ
        public int TitleId { get; set; } // chức danh
        public int PlaceOfWorkId { get; set; } // nơi làm việc
        public int ContractNumber { get; set; } // Phụ lục số mấy rồi
        [MaxLength(250)]
        public string? DecisionNo { get; set; } // số chứng từ quyết định
        public bool IsActive { get; set; } // áp dụng chưa
        [MaxLength(1000)]
        public string? Remark { get; set; } // ghi chú
        [MaxLength(50)]
        public string? StatusCode { get; set; } // trạng thái lấy từ enum
        [MaxLength(250)]
        public string? AuthorizationLetter { get; set; } // giấy ủy quyền
        public bool IsSalaryAdjustment { get; set; } // áp dụng chưa
        [MaxLength(50)]
        public string? TaxTypeCode { get; set; } // loại tính thuế
        public double SalaryCoefficient { get; set; } // Hệ số lương
        [Column(TypeName = "decimal(19, 6)")]
        public decimal TotalSalary { get; set; } // Tổng lương
        [Column(TypeName = "decimal(19, 6)")]
        public decimal NetSalary { get; set; } // Tổng lương thực nhận
        public bool IsCompanyDeduction { get; set; } // công ty đóng trích nộp thay
        public bool IsCompanyInsurance { get; set; } // công ty đóng bảo hiểm thay

    }
}
