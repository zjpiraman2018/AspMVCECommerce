namespace AspMVCECommerce.Models
{
    public class ShippingAddress2
    {
        public string City { get; set; }
        public string CountryCode { get { return "PH"; } }
        public string Line1 { get; set; }
        public string Phone { get; set; }
        public string PostalCode { get; set; }
        public string Line2 { get; set; }
        public bool DefaultAddress { get { return true; } }
        public bool PreferredAddress { get { return true; } }
        public string RecipientName { get; set; }
        public string Status { get { return "CONFIRMED"; } }
        public string Type { get { return "HOME_OR_WORK"; }  }
        public string Province { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}