using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace AspMVCECommerce.ViewModel
{
    public class ProductViewModel
    {
        public ProductViewModel()
        {
            Images = new List<ImageViewModel>();
            Sizes = new List<SizeViewModel>();
            Colors = new List<ColorViewModel>();
        }


        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; }


        [AllowHtml]
        public string Description { get; set; }


        [AllowHtml]
        public string Details { get; set; }

        [Display(Name = "Original Price")]
        [Required]
        public int OriginalPrice { get; set; }

        [Display(Name= "Discounted Price")]
        //[Required]
        public int DiscountedPrice { get; set; }

        [Required]
        public int Stock { get; set; }


        public int CategoryId { get; set; }
        public int BrandId { get; set; }

        [Display(Name = "Promo Sale")]
        [Range(0, 1, ErrorMessage = "Value for {0} must be between {1}.00 to {2}.00 ex. input: 0.55 for 55% discount")]
        public double? PromoSaleOFF { get; set; }

        [Display(Name = "Promo Sale Start Date")]
        public string PromoSaleStartDateTime { get; set; }
        [Display(Name = "Promo Sale End Date")]
        public string PromoSaleEndDateTime { get; set; }

        [Display(Name = "Created Date")]
        public string CreatedDateTime { get; set; }

        public int? AverageRating { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }

        public List<ImageViewModel> Images { get; set; }

        public List<SizeViewModel> Sizes { get; set; }
        public List<ColorViewModel> Colors { get; set; }
    }
}