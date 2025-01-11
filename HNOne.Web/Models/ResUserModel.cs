namespace HNOne.Web.Models
{
    /// <summary>
    /// Model dùng cho lưu vào localstore
    /// </summary>
    public class ResUserModel
    {
        public int branchId { get; set; }
        public string? branchCode { get; set; }
        public string? branchIds { get; set; }
        public int userId { get; set; }
        public int employeeId { get; set; }
        public string? employeeCode { get; set; }
        public string? employeeName { get; set; }
        public int departmentId { get; set; }
        public string? departmentIds { get; set; }
        public string? token { get; set; }
        public string? refreshToken { get; set; }
        public bool isAdmin { get; set; }
    }
}
