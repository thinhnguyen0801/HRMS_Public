
namespace HNOne.Model.Models
{
    public class DailyJobConfigModel
    {
        public int id { get; set; }
        public int docEntry { get; set; }
        public string? objType { get; set; } // loại chứng từ
        public DateTime executeDate { get; set; } // ngày giờ chạy
        public string? sqlText { get; set; } // câu sql
        public bool isCompleted { get; set; } // hoàn thành chứa
        public DateTime? completedDate { get; set; }
        public DateTime? createDate { get; set; }
        public int? userSign { get; set; }
    }
}
