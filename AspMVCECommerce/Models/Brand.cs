using System.ComponentModel.DataAnnotations;

namespace AspMVCECommerce.Models
{
    public class Brand
    {
        [Key]
        public int BrandId { get; set; }
        public string Name { get; set; }
    }

}