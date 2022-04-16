namespace AspMVCECommerce.DTO
{
    public class LineItemDTO
    {

        public int LineItemId { get; set; }

        public int Quantity { get; set; }

        public int TotalPrice { get; set; }

        public int ProductId { get; set; }

        public int? SizeId { get; set; }

        public int? ColorId { get; set; }

        public int ShoppingCartId { get; set; }

    }
}