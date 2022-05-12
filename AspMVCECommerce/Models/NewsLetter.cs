using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AspMVCECommerce.Models
{
    public class NewsLetter
    {
        public int NewsLetterId { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }
    }
}