using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace HNOne.Model.Entities
{
    /// <summary>
    /// Bảng đào tạo
    /// </summary>
    [Table("Trainings")]
    public sealed class Trainings : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string? VoucherNo { get; set; } // số chứng từ
        public int EmployeeSignatureId { get; set; } // nhân viên kí
        public DateTime? DateOfSigning { get; set; } // ngày kí
        public int BranchId { get; set; }
        [MaxLength(250)]
        [Required]
        public string? TrainingCourseName { get; set; } // tên khóa đào tạo
        [MaxLength(50)]
        [Required]
        public string? TypeOfTraning { get; set; } // loại đào tạo (Nội bộ/ ngoài)
        [MaxLength(50)]
        [Required]
        public string? TraningFormatCode { get; set; } // hình thức đào tạo (Bắt buộc,...)
        [MaxLength(50)]
        [Required]
        public string? StatusCode { get; set; } // tình trạng
        [MaxLength(2500)]
        public string? Address { get; set; } // địa điểm
        public DateTime? FromDate { get; set; } // Ngày bắt đầu
        public DateTime? ToDate { get; set; } // Ngày kết thúc 
        public string? Content { get; set; } // Nội dung đào tạo
        public string? Objectives { get; set; } // Mục tiêu đào tạo
        public string? NoteForAll { get; set; } // chi tiết khóa học
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú
    }

    /// <summary>
    /// Bảng lưu danh sách nhân viên trong khóa đạo tạo
    /// </summary>
    [Table("Training1s")]
    public sealed class Training1s
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public int Id { get; set; }
        public int TrainId { get; set; } // id 
        public int EmployeeId { get; set; } // id nhân viên
        public bool IsAbsent { get; set; } // vắng mặt ?
        [MaxLength(250)]
        public string? NoteForAll { get; set; } // đánh giá
        [MaxLength(250)]
        public string? Remark { get; set; } // ghi chú
        public DateTime? DateTracking { get; set; }
        public int? UserSign { get; set; }
    }
}
