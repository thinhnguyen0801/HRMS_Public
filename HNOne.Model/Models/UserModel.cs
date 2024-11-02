
namespace HNOne.Model.Models
{
    public class UserModel : AuditableModel
    {
        public int userId { get; set; }
        public string? userName { get; set; }
        public string? password { get; set; }
        public string? rePassword { get; set; }
        public string? defaultPassword { get; set; } // mật khẩu mặt định
        public string? refreshToken { get; set; }
        public DateTime? refreshTokenExpiryTime { get; set; } // thời hạn hết token
        public int branchId { get; set; }
        public int employeeId { get; set; } // ID nhân viên gắn cho User
        public string? departmentIds { get; set; } // danh sách phòng ban sử dụng
        public string? branchIds { get; set; } // dánh sách chi nhánh được phép sử dụng
        public bool isActive { get; set; }
        public bool isAdmin { get; set; }
        public int perGroupId { get; set; } // phân quyền nhóm
        public string? employeeName { get; set; }
        public string? employeeCode { get; set; }
        public string? branchCode { get; set; }
        public string? branchName { get; set; }
        public string? token { get; set; }

    }
}
