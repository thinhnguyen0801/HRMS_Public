using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Bảng bộ phận
    /// </summary>
    [Table("SubDepartments")]
    public class SubDepartments : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        [MaxLength(50)]
        public string? Code { get; set; }
        [MaxLength(250)]
        public string? Name { get; set; }
        [MaxLength(500)]
        public string? Remark { get; set; }
        public bool IsActive { get; set; } = true;
        public int BranchId { get; set; }
        public int DepartmentId { get; set; }
    }
}
