namespace HNOne.Model.Models
{
    public class AnnualLeaveInfoModel
    {
        public int employeeId { get; set; }
        public string? employeeCode { get; set; }
        public string? employeeName { get; set; }
        public DateTime? probationStartDate { get; set; }
        public int branchId { get; set; }
        public string? branchCode { get; set; }
        public string? branchName { get; set; }
        public int year { get; set; }
        public double numOfLeaveDefault { get; set; } // số ngày phép mặc định trong năm
        public double numOfLeave { get; set; } // số ngày phép trong năm
        public double numOfLeaveLevel { get; set; } // phép thâm niên
        public double numOfLeaveUsed { get; set; } // phép đã sử dụng
        public double numOfLeaveRemaining { get; set; } // phép còn lại
        public double numOfLeavePaid { get; set; } // phép đã thanh toán
        public double numOfLeaveOld { get; set; } // phép của năm củ
        public double numOfLeaveTotal { get; set; } // Tổng số phép
        public DateTime expiryDate { get; set; } // Ngày hết hạn sử dụng phép năm củ
    }
}
