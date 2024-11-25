
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    [Table("CheckInOuts")]
    public sealed class CheckInOuts
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Không tự tăng
        public int Key { get; set; }
        public int Id { get; set; }
        [MaxLength(50)]
        public string? MaChamCong { get; set; }
        public DateTime NgayCham { get; set; }
        public DateTime GioCham { get; set; }
        [MaxLength(50)]
        public string? KieuCham { get; set; }
        [MaxLength(50)]
        public string? NguonCham { get; set; }
        public int MaSoMay { get; set; }
        [MaxLength(50)]
        public string? TenMay { get; set; }
    }
}
