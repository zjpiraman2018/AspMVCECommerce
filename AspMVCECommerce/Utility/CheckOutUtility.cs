using AspMVCECommerce.Models;

namespace AspMVCECommerce.Utility
{
    public static class CheckOutUtility
    {
        public  static void AddCheckOut(CheckOut checkOut, ApplicationDbContext context)
        {

            var tempCustomAddress = checkOut.CustomAddress;
            var tempShippingAddress = checkOut.ShippingAddress;

            checkOut.CustomAddress = null;
            checkOut.ShippingAddress = null;
            checkOut.CheckOutId = 0;

            context.CheckOuts.Add(checkOut);
            context.SaveChanges();

            var checkOutId = checkOut.CheckOutId;
            tempCustomAddress.CheckOutId = checkOutId;
            tempShippingAddress.CheckOutId = checkOutId;

            CustomAddressUtility.AddCustomAddress(tempCustomAddress, context);
            ShippingAddress2Utility.AddShippingAddress2(tempShippingAddress, context);

        }

    }
}