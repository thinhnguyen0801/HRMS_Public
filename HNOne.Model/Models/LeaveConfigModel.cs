

namespace HNOne.Model.Models
{
    public class LeaveConfigModel : AuditableModel
    {
        public int id { get; set; }
        public int year { get; set; } // năm áp dụng
        public DateTime fromDate { get; set; } // ngày bắt đầu
        public DateTime toDate { get; set; } // ngày kết thúc
        public DateTime expiryDate { get; set; } // ngày hết hạn sử dụng phép năm củ
        public int accrualDate { get; set; } // ngày tích phép hàng tháng
        public int numOfLeave { get; set; } // số ngày phép trong năm
        public int numOfYearIncrease { get; set; } // số năm được tăng phép
        public int numOfLeaveIncrease { get; set; } // số phép được tăng
        public int numOfLeaveTransfer { get; set; } // số phép chuyển cho năm sau
        public bool isOffSaturday { get; set; } // được nghỉ T7 không
        public bool isOffSunday { get; set; } // được nghỉ chủ nhật không
        public bool isActive { get; set; }
        public int branchId { get; set; }
        public string? branchName { get; set; }
    }
}
