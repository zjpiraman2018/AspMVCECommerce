using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AspMVCECommerce.Models
{
    public class Image
    {
        public int ImageId { get; set; }
        public string Title { get; set; }

        [DisplayName("Upload File")]
        public string ImagePath { get; set; }

        [NotMapped]
        public HttpPostedFileBase ImageFiIe { get; set; }


        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}