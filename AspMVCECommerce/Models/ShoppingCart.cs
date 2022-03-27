using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspMVCECommerce.Models
{
    public class ShoppingCart
    {
        public ShoppingCart()
        {
            LineItems = new Collection<LineItem>();
        }

        [Key]
        public int ShoppingCartId { get; set; }
        public DateTime Created { get; set; }

        public string CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; }

        public ICollection<LineItem> LineItems { get; set; }
    }

}