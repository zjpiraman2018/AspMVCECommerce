using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspMVCECommerce.Models
{
    public class CustomAddress
    {
        [Key]
        public int CustomAddressId { get; set; }
        public string City { get; set; }
        public string CountryCode { get { return "PH"; } }
        public string Line1 { get; set; }
        public string Phone { get; set; }
        public string PostalCode { get; set; }
        public string Line2 { get; set; }
        public string Province { get; set; }
        public string Status { get { return "CONFIRMED"; } }
        public string Type { get { return "HOME_OR_WORK"; } }


        public int CheckOutId { get; set; }
    }
}