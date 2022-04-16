namespace AspMVCECommerce.Models
{
    public class CheckOut
    {
        public string CountryCode { get { return "PH"; } }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }

        public ShippingAddress2 ShippingAddress { get; set; }
        public CustomAddress CustomAddress { get; set; }
    }
}