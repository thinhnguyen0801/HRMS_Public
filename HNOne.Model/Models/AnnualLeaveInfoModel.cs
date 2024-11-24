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
        public int numOfLeaveDefault { get; set; } // số ngày phép mặc định trong năm
        public int numOfLeave { get; set; } // số ngày phép trong năm
        public int numOfLeaveLevel { get; set; } // phép thâm niên
        public int numOfLeaveUsed { get; set; } // phép đã sử dụng
        public int numOfLeaveRemaining { get; set; } // phép còn lại
        public int numOfLeavePaid { get; set; } // phép đã thanh toán
        public int numOfLeaveOld { get; set; } // phép của năm củ

    }
}
