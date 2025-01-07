namespace HNOne.Model.Models
{
    public class TaxRateModel : AuditableModel
    {
        public int id { get; set; }
        public int branchId { get; set; }
        public string? branchCode { get; set; }
        public string? branchName { get; set; }
        public decimal minSalary { get; set; }
        public decimal maxSalary { get; set; }
        public decimal progressiveAmount { get; set; } // số tiền lũy tiến
        public double taxRate { get; set; } // % thuế
        public int taxBracket { get; set; } // bậc thuế
    }
}
