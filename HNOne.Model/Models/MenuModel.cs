namespace HNOne.Model.Models
{
    public class MenuModel
    {
        public string? menuID { get; set; }
        public string? menuName { get; set; }
        public string? icon { get; set; }
        public string? link { get; set; }
        public string? controller { get; set; }
        public string? parentID { get; set; }
        public string? parentName { get; set; }
        public int level { get; set; }
        public bool isVisible { get; set; }
        public int ordinalNumber { get; set; }
        public string? breadcrumb { get; set; } // đường đẫn
        public int eventId { get; set; }
        public string? actionKey { get; set; }
        public string? actionName { get; set; }
        public bool isAllow { get; set; }
        public List<EventConfigModel>? listEvent { get; set; }
    }

    public class EventConfigModel
    {
        public int eventId { get; set; }
        public string? actionName { get; set; }
        public bool isAllow { get; set; }
    }
}
