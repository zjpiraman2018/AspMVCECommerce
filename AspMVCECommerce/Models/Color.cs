using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AspMVCECommerce.Models
{
    public class Color
    {
        [Key]
        public int ColorId { get; set; }
        public string Name { get; set; }
    }
}