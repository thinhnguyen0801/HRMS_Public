using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Chứng từ quyết định
    /// </summary>
    [Table("DecisionDocuments")]
    public sealed class DecisionDocuments : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string? VoucherNo { get; set; } // số chứng từ
        [MaxLength(50)]
        [Required]
        public string? DecisionTypeCode { get; set; } // loại chứng từ quyết định
        [MaxLength(50)]
        [Required]
        public string? StatusCode { get; set; }
        public int EmployeeId { get; set; } // nhân viên
        public int EmployeeSignatureId { get; set; } // nhân viên kí
        public DateTime? DateOfSigning { get; set; } // ngày kí
        public int BranchId { get; set; }
        [Column(TypeName = "date")]
        public DateTime EffectiveDate { get; set; } // ngày hiệu lực
        [MaxLength(250)]
        public string? ReasonId { get; set; } // lý do
        [MaxLength(250)]
        public string? NoteForAll { get; set; }

        #region Thông tin hiện tại
        public int BranchIdCur { get; set; } // ID chi nhánh
        public int DepartmentIdCur { get; set; } // phòng ban
        public int PositionIdCur { get; set; } // chức vụ
        public int TitleIdCur { get; set; } // chức danh
        public int SubDepartmentIdCur { get; set; } // bộ phận
        public int WorkingBranchIdCur { get; set; } // ID chi nhánh làm việc
        #endregion

        #region Thông tin mới
        public int BranchIdNew { get; set; } // ID chi nhánh
        public int DepartmentIdNew { get; set; } // phòng ban
        public int PositionIdNew { get; set; } // chức vụ
        public int TitleIdNew { get; set; } // chức danh
        public int SubDepartmentIdNew { get; set; } // bộ phận
        public int WorkingBranchIdNew { get; set; } // ID chi nhánh làm việc
        #endregion

    }
}
