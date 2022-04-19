using AspMVCECommerce.Models;

namespace AspMVCECommerce.Utility
{
    public static class ShippingAddress2Utility
    {
        public static int AddShippingAddress2(ShippingAddress2 shippingAddress2, ApplicationDbContext context)
        {
            context.ShippingAddresses.Add(shippingAddress2);
            context.SaveChanges();
            return shippingAddress2.ShippingAddress2Id;
        }
    }
}