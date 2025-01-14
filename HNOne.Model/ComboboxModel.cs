
namespace HNOne.Model
{
    public class ComboboxModel
    {
        public int id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? value { get; set; }
    }

    public class SearchModel
    {
        public int year { get; set; }
        public int month { get; set; }
        public int departmentId { get; set; }
        public int employeeId { get; set; }
        public int branchId { get; set; }
        public string? employeeCode { get; set; }
        public string? employeeName { get; set; }
        public string? statusCode { get; set; }
    }
}
