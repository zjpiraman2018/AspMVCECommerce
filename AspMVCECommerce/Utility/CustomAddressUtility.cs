using AspMVCECommerce.Models;

namespace AspMVCECommerce.Utility
{
    public static class CustomAddressUtility
    {
        public static int AddCustomAddress(CustomAddress customAddress, ApplicationDbContext context)
        {
            context.CustomAddresses.Add(customAddress);
            context.SaveChanges();
            return customAddress.CustomAddressId;
        }
    }
}