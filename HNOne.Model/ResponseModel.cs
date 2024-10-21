
namespace HNOne.Model
{
    public class ResponseModel
    {
        public int status { get; set; }
        public string message { get; set; }
        public object? data { get; set; }
        public ResponseModel()
        {
            status = 200; // success
            message = "";
        }
    }

    /// <summary>
    /// Response generic class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResponseModel<T> where T : class
    {
        public int status { get; set; }
        public string message { get; set; }
        public T? data { get; set; }
        public ResponseModel()
        {
            status = 200; // success
            message = "";
        }
    } 

    public partial class ResCliModel<T> where T : class
    {
        public int status { get; set; }
        public string message { get; set; }
        public IEnumerable<T>? data { get; set; }
        public ResCliModel()
        {
            status = 200; // success
            message = "";
        }
    }    
}
