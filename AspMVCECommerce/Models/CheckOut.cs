using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspMVCECommerce.Models
{
    public class CheckOut
    {

        //[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        //[Key, ForeignKey("ShoppingCart")]

        [Key]
        public int CheckOutId { get; set; }
        public string CountryCode { get { return "PH"; } }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }

        public int ShoppingCartId { get; set; }
        [ForeignKey("ShoppingCartId")]
        public ShoppingCart ShoppingCart { get; set; }

        public ShippingAddress2 ShippingAddress { get; set; }
        public CustomAddress CustomAddress { get; set; }
    }
}