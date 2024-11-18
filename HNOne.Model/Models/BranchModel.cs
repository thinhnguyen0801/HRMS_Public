namespace HNOne.Model.Models
{
    public class BranchModel
    {
        public int branchId { get; set; }
        public string? branchCode { get; set; }
        public string? branchName { get; set; }
        public string? imgUrl { get; set; }
        public string? address { get; set; }
        public string? phoneNumber { get; set; }
        public string? defaultPassword { get; set; }
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
