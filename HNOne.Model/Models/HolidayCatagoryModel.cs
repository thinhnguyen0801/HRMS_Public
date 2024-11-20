
namespace HNOne.Model.Models
{
    public class HolidayCatagoryModel : AuditableModel
    {
        public int id { get; set; }
        public string? name { get; set; } // tên loại ngày nghỉ
        public DateTime fromDate { get; set; }
        public DateTime toDate { get; set; }
        public string? color { get; set; }
        public string? type { get; set; } // loại ngày nghỉ lấy ở enum
        public string? typeName { get; set; } // loại ngày nghỉ lấy ở enum
        public string? remark { get; set; } // ghi chú
    }
}
