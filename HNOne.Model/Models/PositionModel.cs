namespace HNOne.Model.Models
{
    public class PositionModel : AuditableModel
    {
        public int id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? remark { get; set; }
        public bool isActive { get; set; } = true;
        public int branchId { get; set; }
        public string? branchCode { get; set; }
        public string? branchName { get; set; }
        public string? levelCode { get; set; } // cấp độ
        public string? levelName { get; set; } // cấp độ
    }
}
