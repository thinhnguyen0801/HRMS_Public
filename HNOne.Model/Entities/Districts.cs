using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model.Entities
{
    [Table("Districts")]
    public sealed class Districts 
    {
        [Key]
        [MaxLength(50)]
        public string? Code { get; set; }
        [MaxLength(255)]
        public string? Name { get; set; }
        [MaxLength(50)]
        public string? Countrycode { get; set; }
        [MaxLength(50)]
        public string? ProvinceCode { get; set; }
    }
}
