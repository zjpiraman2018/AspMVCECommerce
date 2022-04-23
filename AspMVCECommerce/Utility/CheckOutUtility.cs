using AspMVCECommerce.Models;

namespace AspMVCECommerce.Utility
{
    public static class CheckOutUtility
    {
        public static int AddCheckOut(CheckOut checkOut, ApplicationDbContext context)
        {

            var tempCustomAddress = checkOut.CustomAddress;
            var tempShippingAddress = checkOut.ShippingAddress;


            int customAddressId = CustomAddressUtility.AddCustomAddress(tempCustomAddress, context);
            checkOut.CustomAddressId = customAddressId;

            int? shippingAddress2Id = null;

            if (!string.IsNullOrEmpty(tempShippingAddress.RecipientName))
            {
                shippingAddress2Id = ShippingAddress2Utility.AddShippingAddress2(tempShippingAddress, context);
                checkOut.ShippingAddress2Id = shippingAddress2Id;
            }

            checkOut.CustomAddress = null;
            checkOut.ShippingAddress = null;
            checkOut.CheckOutId = 0;

            context.CheckOuts.Add(checkOut);
            context.SaveChanges();

            return checkOut.CheckOutId;
        }

    }
}