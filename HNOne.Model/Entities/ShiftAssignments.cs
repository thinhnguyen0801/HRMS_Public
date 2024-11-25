using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Bảng phân công ca làm việc
    /// </summary>
    [Table("ShiftAssignments")]
    public class ShiftAssignments : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public long Id { get; set; }
        public int EmployeeId { get; set; }
        public int BranchId { get; set; }
        public int DepartmentId { get; set; }
        public int TitleId { get; set; }
        [MaxLength(50)]
        public string? ShiftCode { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        [MaxLength(50)]
        public string? N01 { get; set; }
        [MaxLength(50)]
        public string? N02 { get; set; }
        [MaxLength(50)]
        public string? N03 { get; set; }
        [MaxLength(50)]
        public string? N04 { get; set; }
        [MaxLength(50)]
        public string? N05 { get; set; }
        [MaxLength(50)]
        public string? N06 { get; set; }
        [MaxLength(50)]
        public string? N07 { get; set; }
        [MaxLength(50)]
        public string? N08 { get; set; }
        [MaxLength(50)]
        public string? N09 { get; set; }
        [MaxLength(50)]
        public string? N10 { get; set; }
        [MaxLength(50)]
        public string? N11 { get; set; }
        [MaxLength(50)]
        public string? N12 { get; set; }
        [MaxLength(50)]
        public string? N13 { get; set; }
        [MaxLength(50)]
        public string? N14 { get; set; }
        [MaxLength(50)]
        public string? N15 { get; set; }
        [MaxLength(50)]
        public string? N16 { get; set; }
        [MaxLength(50)]
        public string? N17 { get; set; }
        [MaxLength(50)]
        public string? N18 { get; set; }
        [MaxLength(50)]
        public string? N19 { get; set; }
        [MaxLength(50)]
        public string? N20 { get; set; }
        [MaxLength(50)]
        public string? N21 { get; set; }
        [MaxLength(50)]
        public string? N22 { get; set; }
        [MaxLength(50)]
        public string? N23 { get; set; }
        [MaxLength(50)]
        public string? N24 { get; set; }
        [MaxLength(50)]
        public string? N25 { get; set; }
        [MaxLength(50)]
        public string? N26 { get; set; }
        [MaxLength(50)]
        public string? N27 { get; set; }
        [MaxLength(50)]
        public string? N28 { get; set; }
        [MaxLength(50)]
        public string? N29 { get; set; }
        [MaxLength(50)]
        public string? N30 { get; set; }
        [MaxLength(50)]
        public string? N31 { get; set; }
    }
}
