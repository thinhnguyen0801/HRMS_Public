using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model.Entities
{
    [Table("Wards")]
    public sealed class Wards 
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
        [MaxLength(50)]
        public string? DistrictCode { get; set; }
    }
}
