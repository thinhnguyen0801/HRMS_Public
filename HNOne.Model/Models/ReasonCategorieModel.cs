namespace HNOne.Model.Models
{
    public class ReasonCategorieModel
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? type { get; set; } // loại lý do
        public bool isActive { get; set; }
        public DateTime? createDate { get; set; }
        public int? userSign { get; set; }
        public DateTime? updateDate { get; set; }
        public int? userSign2 { get; set; }
        public bool isDelete { get; set; }
        public string? deleteReason { get; set; }
        public DateTime? dateTracking { get; set; }
        public string? userSignName { get; set; }
        public string? userSign2Name { get; set; }
    }
}
