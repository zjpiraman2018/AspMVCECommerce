using AspMVCECommerce.Models;
using System;
using System.Linq;

namespace AspMVCECommerce.Utility
{
    public static class ShoppingCartUtility
    {
        public static int GetShoppingCartId(string userId, ApplicationDbContext context)
        {
            var shoppingCart = context.ShoppingCarts.Where(s => s.CustomerId == userId).OrderByDescending(s => s.ShoppingCartId).FirstOrDefault();
            if (shoppingCart == null)
            {
                return AddShoppingCart(userId, context);
            }
            return shoppingCart.ShoppingCartId;
        }

        public static int AddShoppingCart(string userId, ApplicationDbContext context)
        {

            var shoppingCart = new ShoppingCart() { Created = DateTime.Now, CustomerId = userId, ShoppingCartId = 0 };
            context.ShoppingCarts.Add(shoppingCart);
            context.SaveChanges();
            return shoppingCart.ShoppingCartId;
        }
    }
}