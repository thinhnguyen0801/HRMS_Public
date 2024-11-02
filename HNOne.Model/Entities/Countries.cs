using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model.Entities
{
    [Table("Countries")]
    public sealed class Countries 
    {
        [Key]
        [MaxLength(50)]
        public string? Code { get; set; }
        [MaxLength(255)]
        public string? Name { get; set; }
    }
}
