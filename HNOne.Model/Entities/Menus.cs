using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HNOne.Model.Entities
{
    [Table("Menus")]
    public sealed class Menus
    {
        [Key]
        [MaxLength(50)]
        public string? MenuID { get; set; }
        [MaxLength(250)]
        [Required]
        public string? MenuName { get; set; }
        [MaxLength]
        public string? Icon { get; set; }
        [MaxLength(250)]
        public string? Link { get; set; }
        [MaxLength(100)]
        public string? Controller { get; set; }
        [MaxLength(50)]
        public string? ParentID { get; set; }
        public int Level { get; set; }
        public bool IsVisible { get; set; }
        public int OrdinalNumber { get; set; }
    }
}
