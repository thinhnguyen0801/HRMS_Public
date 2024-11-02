using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Tỉnh/Thành phố
    /// </summary>
    [Table("Provinces")]
    public sealed class Provinces 
    {
        [Key]
        [MaxLength(50)]
        public string? Code { get; set; }
        [MaxLength(255)]
        public string? Name { get; set; }
        [MaxLength(50)]
        public string? ZipcCode { get; set; }
        [MaxLength(50)]
        public string? Countrycode { get; set; }
    }
}
