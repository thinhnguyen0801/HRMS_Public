
namespace HNOne.Model
{
    public class RequestModel
    {
        public int documentId { get; set; } = -1;
        public int employeeId { get; set; } = -1;
        public int userId { get; set; }
        public int branchId { get; set; }
        public string? token { get; set; }
        public string? process { get; set; }
        public string? json { get; set; }
        public string? jsonDetail { get; set; }
        public string? type { get; set; }
        public string? opt { get; set; }
        public string? opt1 { get; set; }
        public string? opt2 { get; set; }
        public string? opt3 { get; set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
    }   
}
