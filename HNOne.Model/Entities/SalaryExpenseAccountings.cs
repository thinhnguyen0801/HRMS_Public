
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Hạch toán chi phí lương
    /// </summary>
    [Table("SalaryExpenseAccountings")]
    public sealed class SalaryExpenseAccountings : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string? VoucherNo { get; set; } // số chứng từ
        [MaxLength(50)]
        [Required]
        public string? StatusCode { get; set; }
        public int EmployeeSignatureId { get; set; } // nhân viên kí
        public DateTime? DateOfSigning { get; set; } // ngày kí
        public int BranchId { get; set; }
        public int Month {  get; set; }
        public int Year {  get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DueDate { get; set; }
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú
        [Column(TypeName = "decimal(19, 6)")]
        public decimal DocTotal { get; set; }
    }

    [Table("SalaryExpenseAccounting1s")]
    public sealed class SalaryExpenseAccounting1s
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public int Id { get; set; }
        public int SalaryExpenseAccountingId { get; set; }
        public int LineId { get; set; }
        [MaxLength(250)]
        public string? SalaryCatagoryCode { get; set; }
        [MaxLength(250)]
        public string? SalaryCatagoryName { get; set; }
        [MaxLength(50)]
        public string? Account1 { get; set; }
        [MaxLength(50)]
        public string? Account2 { get; set; }
        [Column(TypeName = "decimal(19, 6)")]
        public decimal LineTotal { get; set; }
        public DateTime? DateTracking { get; set; }
        public int? UserSign { get; set; }
    }
}
