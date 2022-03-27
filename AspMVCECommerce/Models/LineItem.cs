using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspMVCECommerce.Models
{
    public class LineItem
    {

        [Key]
        public int LineItemId { get; set; }

        public int Quantity { get; set; }

        public int TotalPrice { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public int? SizeId { get; set; }
        [ForeignKey("SizeId")]
        public Size Size { get; set; }

        public int? ColorId { get; set; }
        [ForeignKey("ColorId")]
        public Color Color { get; set; }


        public int ShoppingCartId { get; set; }
        [ForeignKey("ShoppingCartId")]
        public ShoppingCart ShoppingCart { get; set; }
    }
}