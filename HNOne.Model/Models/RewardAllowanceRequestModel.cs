
namespace HNOne.Model.Models
{
    /// <summary>
    /// Thông tin khen thưởng & phụ cấp
    /// </summary>
    public class RewardAllowanceRequestModel : AuditableModel
    {
        public int id { get; set; }
        public string? voucherNo { get; set; } // số chứng từ
        public int salaryConfigId { get; set; } // Loại cấu hình lương
        public string? salaryConfigName { get; set; } // Tên lại cấu hình lương
        public string? rewardName { get; set; } // Tên đợt
        public int employeeSignatureId { get; set; } // nhân viên kí
        public string? employeeSignatureCode { get; set; }
        public string? employeeSignatureName { get; set; }
        public DateTime rewardDate { get; set; } // tháng thưởng
        public DateTime rewardPaymentDate { get; set; } // tháng chi thưởng
        public DateTime? dateOfSigning { get; set; } // Ngày kí
        public string? statusCode { get; set; } // trạng thái lấy từ enum
        public string? statusName { get; set; } // trạng thái lấy từ enum
        public int branchId { get; set; }
        public string? noteForAll { get; set; } // nội dung khen thưởng
        public string? link { get; set; }
        public string? jsonDetail { get; set; } // danh sách chi tiết
        public decimal totalReward { get; set; } // tổng tiền
    }

    /// <summary>
    /// Thông tin line khen thưởng & phụ cấp
    /// </summary>
    public class RewardAllowanceRequest1Model
    {
        public int id { get; set; }
        public int rewardAllowanceId { get; set; }
        public int employeeId { get; set; } // nhân viên
        public string? employeeCode { get; set; }
        public string? employeeName { get; set; }
        public int departmentId { get; set; } // phòng ban
        public string? departmentCode { get; set; }
        public string? departmentName { get; set; }
        public string? salaryCalculateMethod { get; set; } // cách tính lương lấy ở enum
        public string? salaryCalculateMethodName { get; set; } // cách tính lương lấy ở enum
        public decimal rewardAmount { get; set; } // tiền thưởng
        public decimal taxPayment { get; set; } // tiền thuế phải đóng
        public decimal paidAmount { get; set; } // số tiền chi trả cho nhân viên
        public decimal totalSalary { get; set; } // Tổng tiền
        public decimal netSalary { get; set; } // số tiền thực lãnh
        public decimal maxSalary { get; set; } // số tiền thực lãnh tối đa được nhận
        public string? remark { get; set; } // ghi chú cho từng nhân viên
        public double cDM { get; set; } // Công định mức
        public double cTT { get; set; } // Công thực tế
        public DateTime? dateTracking { get; set; }
        public int? userSign { get; set; }
    }
}
