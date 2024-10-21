
namespace HNOne.Model.Entities
{
    public interface IAuditable
    {
        DateTime? CreateDate { get; set; }
        int? UserSign { get; set; }
        DateTime? UpdateDate { get; set; }
        int? UserSign2 { get; set; }
        bool IsDelete { get; set; }
        string? DeleteReason { get; set; }
        DateTime? DateTracking { get; set; }
    }
    public class Auditable : IAuditable
    {
        public DateTime? CreateDate { get; set; }
        public int? UserSign { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UserSign2 { get; set; }
        public bool IsDelete { get; set; }
        public string? DeleteReason { get; set; }
        public DateTime? DateTracking { get; set; }
    }
}
