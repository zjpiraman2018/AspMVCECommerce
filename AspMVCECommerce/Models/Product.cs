using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspMVCECommerce.Models
{
    public class Product
    {

        public Product()
        {
            Reviews = new Collection<Review>();
            Images = new Collection<Image>();
            Sizes = new Collection<Size>();
            Colors = new Collection<Color>();
        }

        [Key]
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
        
        public int OriginalPrice { get; set; }
        public int DiscountedPrice { get; set; }
        public int Stock { get; set; }


        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }


        public ICollection<Size> Sizes { get; set; }
        public ICollection<Color> Colors { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Image> Images { get; set; }
    }
}