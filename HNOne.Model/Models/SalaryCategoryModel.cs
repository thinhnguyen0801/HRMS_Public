
namespace HNOne.Model.Models
{
    public class SalaryCategoryModel : AuditableModel
    {
        public int id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public int rowOrder { get; set; } // đánh số tt
        public bool isActive { get; set; } = true;
    }
}
