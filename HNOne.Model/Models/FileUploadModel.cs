
namespace HNOne.Model.Models
{
    public class FileUploadModel
    {
        public int id { get; set; }
        public int docEntry { get; set; }
        public string? filePath { get; set; }
        public string? fileName { get; set; }
        public string? imageUrl { get; set; }
        public bool isDelete { get; set; }
    }
}
