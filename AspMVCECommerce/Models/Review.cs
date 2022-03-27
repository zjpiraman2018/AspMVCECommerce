using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspMVCECommerce.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}