namespace HNOne.Web.Models
{
    public class BreadcrumbModel
    {
        public string title { get; set; }
        public string enpoint { get; set; }
        public bool isActive { get; set; }

        public BreadcrumbModel(string title, string enpoint)
        {
            this.title = title;
            this.enpoint = enpoint;
        }
    }
}
