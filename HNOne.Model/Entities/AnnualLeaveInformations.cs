using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model.Entities
{
    [Table("AnnualLeaveInformations")]
    public sealed class AnnualLeaveInformations : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Không tự tăng
        public int Id { get; set; }
        public int EmployeeId { get; set; } // nhân viên
        public int BranchId { get; set; } // chi nhánh 
        public int Month { get; set; }
        public int Year { get; set; }
        public double NumOfLeaveDefault { get; set; }
        public double NumOfLeave { get; set; } // số ngày phép trong năm
        public double NumOfLeaveLevel { get; set; } // phép thâm niên
        public double NumOfLeaveUsed { get; set; } // phép đã sử dụng
        public double NumOfLeaveRemaining { get; set; } // phép còn lại
        public double NumOfLeavePaid { get; set; } // phép đã thanh toán
        public double NumOfLeaveOld { get; set; } // phép của năm củ
    }
}
