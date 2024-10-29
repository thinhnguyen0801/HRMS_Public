
namespace HNOne.Model.Models
{
    public class SalaryConfigurationModel
    {
        public int id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public int branchId { get; set; }
        public bool isActive { get; set; } // áp dụng chưa
        public bool isCalculatePersonalIncomeTax { get; set; } // Tính thuế TNCN ?
        public decimal taxLimit { get; set; } // giới hạn thuế
        public bool bHXH { get; set; }
        public bool bHYT { get; set; }
        public bool bHTN { get; set; }
        public bool bHTNLD { get; set; }
        public double overtimeCoefficient { get; set; } // hệ số tăng ca
        public int salaryCalculateMethod { get; set; } // cách tính lương
    }
}
