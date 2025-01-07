namespace HNOne.Model.Models
{
    public class MonthlySalaryModel
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
        public string? taxTypeName { get; set; } // loại tính thuế
        public int month { get; set; }
        public int year { get; set; }
        public double salaryCoefficient { get; set; } // Hệ số lương
        public decimal lCB { get; set; } // lương cơ bản
        public decimal lQD { get; set; } // lương quyết định
        public double tNC { get; set; } // tổng ngày công
        public double cDM { get; set; } // công định mức của tháng
        public double gCDM { get; set; } // giờ công định mức của tháng
        public double cTT { get; set; } // công thực tế
        public double gCTT { get; set; } // giờ công thực tế
        public decimal lCTT { get; set; } // lương công thực tế
        public double nPN { get; set; } // nghỉ phép năm
        public decimal lCPN { get; set; } // Lương công phép năm
        public double nCD { get; set; } // nghỉ chế độ
        public decimal lCCD { get; set; } // lương công chế độ
        public double cTPC { get; set; } // số công tính phụ cấp
        public double sGT { get; set; } // số giờ thiếu
        public decimal lCT { get; set; } // lương giờ thiếu
        public decimal lPC { get; set; } // lương phụ cấp
        
        public double nL { get; set; } // nghỉ lễ
        public decimal lNL { get; set; } // lương nghỉ lễ

        public double tGTC { get; set; } // số giờ tăng ca
        public double sGTCTC { get; set; } // số giờ tăng ca tiêu chuẩn
        public double sGTCTT { get; set; } // số giờ tăng ca của tháng trước
        public double sGTCKT { get; set; } // số giờ tăng ca được chuyển sang tháng tiếp theo
        public decimal lTC { get; set; } // lương tăng ca
        public decimal tL { get; set; } // tổng lương
        public decimal lCTTNCN { get; set; } // tổng lương
        public decimal lDTTNCN { get; set; } // lương đóng thuế
        public decimal tNNCN { get; set; } // lương đóng thuế
        public decimal bhxh { get; set; } // lương đóng thuế
        public decimal bhyt { get; set; } // lương đóng thuế
        public decimal bhtn { get; set; } // lương đóng thuế
        public bool isLocked { get; set; }
        public bool isCompanyDeduction { get; set; }
        public bool isCompanyInsurance { get; set; }
        public decimal lNet { get; set; } // lương đóng thuế
    }
}
