using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspMVCECommerce.Models
{
    public class Log
    {
        public int LogId { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
    }
}