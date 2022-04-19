using AspMVCECommerce.Models;
using System;

namespace AspMVCECommerce.Utility
{
    public static class OrderUtility
    {

        public static int AddOrder(int ShoppingCartId, int lineItemsCount,int totalAmount, string userId, ApplicationDbContext context)
        {
            var order = new Order() {CreatedDate = DateTime.Now, PaymentMethod ="Paypal", PaymentStatus = "Paid", OrderStatus = "Shipped" , LineItemsCount = lineItemsCount, TotalAmount = totalAmount, ShoppingCartId = ShoppingCartId };
            context.Orders.Add(order);
            context.SaveChanges();
            return order.OrderId;
        }

    }
}