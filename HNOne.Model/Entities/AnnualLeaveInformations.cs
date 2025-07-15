using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Bảng thông tin phép năm của nhân viên
    /// </summary>
    [Table("AnnualLeaveInformations")]
    public sealed class AnnualLeaveInformations : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Không tự tăng
        public int Id { get; set; }
        public int EmployeeId { get; set; } // nhân viên
        public int BranchId { get; set; } // chi nhánh 
        public int Year { get; set; }
        public int LeaveConfigId { get; set; } // Id cấu hình
        public DateTime ExpiryDate { get; set; } // ngày hết hạn sử dụng phép năm củ
        public double NumOfLeaveDefault { get; set; } // số ngày phép năm mặc định
        public double NumOfLeave { get; set; } // số ngày phép được sử dụng trong năm
        public double NumOfLeaveLevel { get; set; } // phép thâm niên
        public double NumOfLeaveUsed { get; set; } // phép đã sử dụng
        public double NumOfLeaveRemaining { get; set; } // phép còn lại
        public double NumOfLeavePaid { get; set; } // phép đã thanh toán
        public double NumOfLeaveOld { get; set; } // phép của năm củ
        public int AccrualDate { get; set; } // ngày tích phép hàng tháng
        public double NumOfYearIncrease { get; set; } // số năm được tăng phép
        public double NumOfLeaveIncrease { get; set; } // số phép được tăng
        public double NumOfLeaveTransfer { get; set; } // số phép chuyển cho năm sau
    }
}
