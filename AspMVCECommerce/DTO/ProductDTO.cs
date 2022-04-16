using System.Collections.Generic;

namespace AspMVCECommerce.DTO
{
    public class ProductDTO
    {
        public ProductDTO()
        {
            Sizes = new List<SizeDTO>();
            Colors = new List<ColorDTO>();
        }

        public int ProductId { get; set; }

        public int Stock { get; set; }

        public int OriginalPrice { get; set; }
        public int DiscountedPrice { get; set; }

        public string ProductName { get; set; }

        public List<SizeDTO> Sizes { get; set; }
        public List<ColorDTO> Colors { get; set; }

        public string ImagePath { get; set; }
        public bool IsPromo { get; set; }

        public string Category { get; set; }
        public string Brand { get; set; }
    }
}