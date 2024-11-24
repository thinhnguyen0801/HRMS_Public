namespace HNOne.Model.Models
{
    public class NotificationModel
    {
        public int id { get; set; }
        public int branchId { get; set; }
        public int docEntry { get; set; }
        public string? voucherNo { get; set; } // mã chứng từ
        public string? objType { get; set; } // loại chứng từ
        public string? objTypeName { get; set; } // loại chứng từ
        public string? statusCode { get; set; } // trạng thái phê duyệt
        public int employeeId { get; set; } // gửi tới ai
        public string? message { get; set; } // thông báo là gì
        public bool isView { get; set; } // đã xem chưa
        public DateTime? createDate { get; set; } // ngày giờ tạo
        public int? userSign { get; set; } // người tạo
        public int totalRow { get; set; } // tổng số thông báo
    }
}
