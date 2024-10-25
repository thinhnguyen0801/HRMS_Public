
namespace HNOne.Model.Entities
{
    public class PermissionGroups : Auditable
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
    }
}
