using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Common
{
    public class CommonConstants
    {
        #region
        public const string STATUS_CODE_ADD = "A"; // TẠO MỚI
        public const string STATUS_CODE_APPROVED = "D"; // ĐÃ ĐƯỢC PHÊ DUYỆT
        public const string STATUS_CODE_APPROVAL_PENDING = "Y"; // ĐÃ GỬI YÊU CẦU PHÊ DUYỆT
        public const string STATUS_CODE_DENY = "T"; // ĐÃ TỪ CHỐI
        public const string STATUS_CODE_CANCEL = "C"; // ĐÃ HỦY
        public const string STATUS_CODE_WAIT_APPROVAL = "WAIT"; // CHỜ DUYỆT
        #endregion
    }
}
