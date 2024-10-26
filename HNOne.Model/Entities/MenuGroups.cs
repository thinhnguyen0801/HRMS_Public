namespace HNOne.Model.Entities
{
    public class MenuGroups
    {
        public int Id { get; set; }
        public string? PerGrpCode { get; set; } // Permission Group
        public string? MenuCode { get; set; }
        public int? UserSign { get; set; }
        public DateTime? DateTracking { get; set; }
    }
}
