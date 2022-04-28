using AspMVCECommerce.Models;
using System.Data.Entity;

namespace AspMVCECommerce.Utility
{
    public static class ProductUtility
    {

        public static void UpdateProductSold(int productId, int quantity, ApplicationDbContext context)
        {
            var product = context.Products.Find(productId);
            product.Stock = product.Stock - quantity;
            product.Sold = (product.Sold == null ? 0: product.Sold) + quantity;
            context.Entry(product).State = EntityState.Modified;
            context.SaveChanges();
        }

    }

}