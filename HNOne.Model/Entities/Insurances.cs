using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    [Table("Insurances")]
    public class Insurances : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        public int EmployeeId { get; set; } // mã nhân viên
        [MaxLength(50)]
        public string? InsuranceType { get; set; } // Loại bảo hiểm khai ở enum
        [MaxLength(50)]
        public string? InsuranceNo { get; set; } // Số bảo hiểm
        public DateTime? StartDate { get; set; } // Ngày cấp
        public DateTime? EndDate { get; set; } // Ngày hết hạn
        [Column(TypeName = "decimal(19, 6)")]
        public decimal Rate { get; set; } // tỉ lệ đóng BH
        [MaxLength(250)]
        public string? ZipCode { get; set; } // mã tỉnh cấp
        [MaxLength(250)]
        public string? Address { get; set; } // Nơi đăng kí khám chửa bệnh
        [MaxLength(250)]
        public string? AddressNo { get; set; } // mã nơi đăng kí
    }
}
