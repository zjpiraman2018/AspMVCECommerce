namespace AspMVCECommerce.DTO
{
    public class LineItemViewDTO
    {

        public int LineItemId { get; set; }

        public int Quantity { get; set; }

        public int TotalPrice { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public int? SizeId { get; set; }

        public int? ColorId { get; set; }

        public string SizeName { get; set; }

        public string ColorName { get; set; }

        public int ShoppingCartId { get; set; }

        public string ImagePath { get; set; }
    } 
}