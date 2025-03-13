using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HNOne.Model.Entities
{
    [Table("LeaveConfigs")]
    public sealed class LeaveConfigs : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        public int Year { get; set; } // năm áp dụng
        public DateTime FromDate { get; set; } // ngày bắt đầu
        public DateTime ToDate { get; set; } // ngày kết thúc
        public DateTime ExpiryDate { get; set; } // ngày hết hạn sử dụng phép năm củ
        public int AccrualDate { get; set; } // ngày tích phép hàng tháng
        public int NumOfLeave { get; set; } // số ngày phép trong năm
        public int NumOfYearIncrease { get; set; } // số năm được tăng phép
        public int NumOfLeaveIncrease { get; set; } // số phép được tăng
        public int NumOfLeaveTransfer { get; set; } // số phép chuyển cho năm sau
        public bool IsOffSaturday { get; set; } // được nghỉ T7 không
        public bool IsOffSunday { get; set; } // được nghỉ chủ nhật không
        public bool IsActive { get; set; }
        public int BranchId { get; set; }
    }
}
