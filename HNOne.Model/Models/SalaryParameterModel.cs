namespace HNOne.Model.Models
{
    public class SalaryParameterModel : AuditableModel
    {
        public int id { get; set; }
        public int branchId { get; set; }
        public string? branchName { get; set; }
        public bool isActive { get; set; } // áp dụng chưa
        public decimal taxSalary { get; set; } // lương chịu thuế
        public decimal taxSalaryProbationary { get; set; } // lương chịu thuế thử việc
        public decimal salaryFamilyCircumstanceDeduction { get; set; } // mức giảm trừ gia cảnh
        public DateTime? fromDate { get; set; } // hiệu lực từ ngày
        public DateTime? toDate { get; set; } // hiệu lực đến ngày
    }
}
