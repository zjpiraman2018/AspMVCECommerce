using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspMVCECommerce.Models
{
    public class Order
    {
        public DateTime CreatedDate { get; set; }
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public int LineItemsCount { get; set; }
        public int TotalAmount { get; set; }
        public string OrderStatus { get; set; }

        public int ShoppingCartId { get; set; }
        [ForeignKey("ShoppingCartId")]
        public ShoppingCart ShoppingCart { get; set; }
    }
}