using System;
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
        }

        [Key]
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
        [DisplayFormat(DataFormatString = "₱{0:#,0}", ApplyFormatInEditMode = true)]
        public int OriginalPrice { get; set; }
        [DisplayFormat(DataFormatString = "₱{0:#,0}", ApplyFormatInEditMode = true)]
        public int DiscountedPrice { get; set; }
        public int Stock { get; set; }

        public double PromoSaleOFF { get; set; }
        public DateTime? PromoSaleStartDateTime { get; set; }
        public DateTime? PromoSaleEndDateTime { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }


        public int BrandId { get; set; }
        [ForeignKey("BrandId")]
        public Brand Brand { get; set; }

        public int? AverageRating { get; set; }


        public ICollection<Review> Reviews { get; set; }
        public ICollection<Image> Images { get; set; }


    }
}