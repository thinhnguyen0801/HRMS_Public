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
    /// Bảng hợp đồng
    /// </summary>
    [Table("Contracts")]
    public class Contracts : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int BranchId { get; set; }
        public int TimesheetId { get; set; } // ID bảng công
        [MaxLength(250)]
        public string? ContractCode { get; set; } // số hợp đồng
        public DateTime? StartDate { get; set; } // Ngày bắt đầu
        public DateTime? EndDate { get; set; } // Ngày kết thúc
        public DateTime? DateOfSigning { get; set; } // Ngày kí
        public DateTime? DeductionDate { get; set; } // Ngày trích nộp
        public int EmployeeSignatureId { get; set; } // nhân viên kí
        public int ContractTypeId { get; set; } // Loại hợp đồng
        public int PositionId { get; set; } // chức vụ
        public int TitleId { get; set; } // chức danh
        public int ContractNumber { get; set; } // kí lần thứ mấy rồi
        [MaxLength(1000)]
        public string? Remark { get; set; } // ghi chú
        [MaxLength(50)]
        public string? StatusCode { get; set; } // trạng thái lấy từ enum
        [MaxLength(50)]
        public string? TaxTypeCode { get; set; } // loại tính thuế
        public double SalaryCoefficient { get; set; } // Hệ số lương
        [Column(TypeName = "decimal(19, 6)")]
        public decimal TotalSalary { get; set; } // Tổng lương
        [Column(TypeName = "decimal(19, 6)")]
        public decimal NetSalary { get; set; } // Tổng lương thực nhận
        public double NumberOfMonths { get; set; } // số tháng
        public int NumberOfDaysReduced { get; set; } // số ngày giảm
        [MaxLength(250)]
        public string? DecisionNo { get; set; } // số chứng từ quyết định
        public int PlaceOfWorkId { get; set; } // nơi làm việc
        public bool IsActive { get; set; } // áp dụng chưa
        public bool IsCompanyDeduction { get; set; } // công ty đóng trích nộp thay
        public bool IsCompanyInsurance { get; set; } // công ty đóng bảo hiểm thay
        
        
    }
}
