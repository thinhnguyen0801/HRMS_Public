
namespace HNOne.Model.Models
{
    /// <summary>
    /// Hạch toán chi phí lương
    /// </summary>
    public class SalaryExpenseAccountingModel : AuditableModel
    {
        public int id { get; set; }
        public string? voucherNo { get; set; } // số chứng từ
        public string? statusCode { get; set; }
        public string? statusName { get; set; } // trạng thái lấy từ enum
        public int employeeSignatureId { get; set; } // nhân viên kí
        public string? employeeSignatureCode { get; set; }
        public string? employeeSignatureName { get; set; }
        public DateTime? dateOfSigning { get; set; } // ngày kí
        public int branchId { get; set; }
        public string? branchName { get; set; }
        public string? salaryPreiod { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public DateTime? docDate { get; set; }
        public DateTime? dueDate { get; set; }
        public string? remark { get; set; } // ghi chú
        public decimal docTotal { get; set; }
        public string? link { get; set; }
        public string? jsonDetail { get; set; }
    }

    public class SalaryExpenseAccounting1Model
    {
        public int id { get; set; }
        public int lineId { get; set; }
        public int salaryExpenseAccountingId { get; set; }
        public string? salaryCatagoryCode { get; set; }
        public string? salaryCatagoryName { get; set; }
        public string? account1 { get; set; }
        public string? account2 { get; set; }
        public decimal lineTotal { get; set; }
        public DateTime? dateTracking { get; set; }
        public int? userSign { get; set; }
    }
}
