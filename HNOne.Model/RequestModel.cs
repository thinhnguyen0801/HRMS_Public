using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model
{
    public class RequestModel
    {
        public string? userId { get; set; }
        public string? token { get; set; }
        public string? process { get; set; }
        public string? json { get; set; }
    }   
}
