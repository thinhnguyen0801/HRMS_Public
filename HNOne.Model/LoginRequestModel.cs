using System.ComponentModel.DataAnnotations;

namespace HNOne.Model
{
    public class LoginRequestModel
    {
        [Required(ErrorMessage = "Vui lòng chọn [Chi nhánh]")]
        public string? branchId { get; set; }

        [Required(ErrorMessage = "Vui lòng điền Tên đăng nhập")]
        public string? userName { get; set; }

        [Required(ErrorMessage = "Vui lòng điền Mật khẩu")]
        public string? password { get; set; }
        public string? refreshToken { get; set; }
        public bool rememberMe { get; set; }
    }
}
