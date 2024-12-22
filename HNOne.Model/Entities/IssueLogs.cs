using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model.Entities
{
    [Table("Branchs")]
    public sealed class IssueLogs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public int Id { get; set; }
        public DateTime DateTracking { get; set; }
        [Required]
        [MaxLength(50)]
        public string? Level { get; set; }
        [Required]
        [MaxLength(50)]
        public string? LogEvent { get; set; }
        [Required]
        public string? Message { get; set; }
        public string? Exception{ get; set; }
        [MaxLength(20)]
        public string? HostAddress { get; set; }
        [MaxLength(50)]
        public int UserId { get; set; }
        [MaxLength(200)]
        public string? Browser { get; set; }
        [MaxLength(200)]
        public string? Url { get; set; }
    }
}
