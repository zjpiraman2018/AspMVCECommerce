using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AspMVCECommerce.Models
{
    public class WishList
    {
        public int WishListId { get; set; }

        public int ProductId { get; set; }

        public string CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }
    }
}