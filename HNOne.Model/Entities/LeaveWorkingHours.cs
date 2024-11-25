using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    [Table("LeaveWorkingHours")]
    public sealed class LeaveWorkingHours : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string? VoucherNo { get; set; } // số chứng từ
        public int EmployeeId { get; set; } // nhân viên
        public int EmployeeSignatureId { get; set; } // nhân viên kí
        public DateTime? DateOfSigning { get; set; } // ngày kí
        public int BranchId { get; set; } // chi nhánh
        public int DepartmentId { get; set; } // chức vụ
        [MaxLength(50)]
        [Required]
        public string? StatusCode { get; set; }
        public DateTime? FromDate { get; set; } // Ngày bắt đầu
        public DateTime? ToDate { get; set; } // Ngày kết thúc
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú
        [MaxLength(50)]
        [Required]
        public string? RequestType { get; set; } // loại yêu cầu
        public double TotalHours { get; set; } // tổng số giờ
    }
}
