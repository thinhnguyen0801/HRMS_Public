
namespace HNOne.Common
{
    public class CommonConstants
    {
        #region Tình trạng chứng từ
        public const string STATUS_CODE_ADD = "A"; // TẠO MỚI
        public const string STATUS_CODE_APPROVED = "D"; // ĐÃ ĐƯỢC PHÊ DUYỆT
        public const string STATUS_CODE_APPROVAL_PENDING = "Y"; // ĐÃ GỬI YÊU CẦU PHÊ DUYỆT
        public const string STATUS_CODE_DENY = "T"; // ĐÃ TỪ CHỐI
        public const string STATUS_CODE_CANCELED = "C"; // ĐÃ HỦY
        public const string STATUS_CODE_EXPIRED = "E"; // HẾT HẠN/HẾT HIỆU LỰC
        #endregion

        #region Cấu hình thông tin
        public const string WORK_TYPE_DEFAULT = "DEFAULT";
        public const string MAX_OVERTIME_REQUEST = "QuyDinhSoGioTangCa";
        public const string ALLOW_UPDATE_EMPLOYEE_INFO = "CapNhatThongTinNhanVien";
        public const string ENUM_ACTIVE = "ACTIVE";
        public const string ENUM_FILTER = "FILTER";
        public const string ENUM_CATAGORY = "ENUM_CATAGORY";
        public const string ENUM_ALLOW_EDIT = "AllowEdit";
        public const string ENUM_PAYMENT_TYPE_TM = "TM";
        public const string ENUM_PAYMENT_TYPE_UNC = "UNC";
        public const string ENUM_PAYMENT_REQUEST_TYPE_CHILUONG = "CHILUONG";
        public const string ENUM_PAYMENT_REQUEST_TYPE_NOPBAOHIEM = "NOPBAOHIEM";
        public const string ENUM_PAGE_LOGIN = "LOGIN";
        public const string ENUM_LIST = "LIST";
        public const string ENUM_BY_EMPLOYEE = "BY_EMPLOYEE";
        public const string ENUM_DETAIL = "DETAIL";
        public const string ENUM_EMPLOYEE_SIGNATURE = "EMPLOYEE_SIGNATURE";
        public const string ENUM_BASIC_SALARY = "LCB";
        public const string ENUM_NEGOTIATED_SALARY = "LQD";
        public const string ENUM_ALLOWANCE_SALARY = "LPC";
        #endregion
    }
}
