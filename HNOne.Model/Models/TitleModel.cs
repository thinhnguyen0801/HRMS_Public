namespace HNOne.Model.Models
{
    public class TitleModel : AuditableModel
    {
        public int id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? remark { get; set; }
        public bool isActive { get; set; } = true;
        public int branchId { get; set; }
        public string? branchCode { get; set; }
        public string? branchName { get; set; }
        public int departmentId { get; set; }
        public string? departmentCode { get; set; }
        public string? departmentName { get; set; }
    }
}
