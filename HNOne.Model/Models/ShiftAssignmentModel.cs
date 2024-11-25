namespace HNOne.Model.Models
{
    public class ShiftAssignmentModel : AuditableModel
    {
        public int rowOrder { get; set; } // số thứ tự
        public int employeeId { get; set; }
        public string? employeeCode { get; set; } 
        public string? employeeName { get; set; } 
        public int branchId { get; set; }
        public string? branchCode { get; set; }
        public string? branchName { get; set; }
        public int departmentId { get; set; } // phòng ban
        public string? departmentName { get; set; }
        public int titleId { get; set; } // chức danh
        public string? titleCode { get; set; } // chức danh
        public string? titleName { get; set; } //
        public string? shiftCode { get; set; } // ca làm việc mặc định của nhân viên
        public string? n01 { get; set; }
        public string? n02 { get; set; }
        public string? n03 { get; set; }
        public string? n04 { get; set; }
        public string? n05 { get; set; }
        public string? n06 { get; set; }
        public string? n07 { get; set; }
        public string? n08 { get; set; }
        public string? n09 { get; set; }
        public string? n10 { get; set; }
        public string? n11 { get; set; }
        public string? n12 { get; set; }
        public string? n13 { get; set; }
        public string? n14 { get; set; }
        public string? n15 { get; set; }
        public string? n16 { get; set; }
        public string? n17 { get; set; }
        public string? n18 { get; set; }
        public string? n19 { get; set; }
        public string? n20 { get; set; }
        public string? n21 { get; set; }
        public string? n22 { get; set; }
        public string? n23 { get; set; }
        public string? n24 { get; set; }
        public string? n25 { get; set; }
        public string? n26 { get; set; }
        public string? n27 { get; set; }
        public string? n28 { get; set; }
        public string? n29 { get; set; }
        public string? n30 { get; set; }
        public string? n31 { get; set; }
        public int month { get; set; }
        public int year { get; set; }
    }
}
