using AspMVCECommerce.DTO;
using AspMVCECommerce.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;



namespace AspMVCECommerce.Controllers
{
    public class ProductsWAController : ApiController
    {

        private ApplicationDbContext context = new ApplicationDbContext();

        [System.Web.Http.HttpGet]
        public IHttpActionResult GetProductDetails(int? productId)
        {
            if (productId == null)
            {
                return Content(HttpStatusCode.BadRequest, "productId is required!");
            }

            try
            {
                Product product = context.Products.Include(p => p.Category).Include(p => p.Brand).Include(p => p.Images).Include(p=>p.Sizes).Include(p=>p.Colors).Single(p => p.ProductId == productId);
                var productDTO = new ProductDTO();
                productDTO.ProductId = product.ProductId;
                productDTO.Colors = product.Colors.ToList().Select(c => new ColorDTO() { Name = c.Name, ColorId = c.ColorId  }).ToList();
                productDTO.Sizes = product.Sizes.ToList().Select(c => new SizeDTO() { Name = c.Name, SizeId = c.SizeId }).ToList();
                productDTO.ImagePath = product.Images.Where(i => i.Default = true).FirstOrDefault().ImagePath;
                productDTO.DiscountedPrice = product.DiscountedPrice;
                productDTO.OriginalPrice = product.OriginalPrice;
                productDTO.Stock = product.Stock;
                productDTO.ProductName = product.Name;
                productDTO.Category = product.Category.Name;
                productDTO.Brand = product.Brand.Name;

                productDTO.IsPromo = false;
                if (product.PromoSaleOFF > 0)
                {
                    if (DateTime.Now >= product.PromoSaleStartDateTime && DateTime.Now <= product.PromoSaleEndDateTime)
                    {

                        productDTO.IsPromo = true;
                    }
                }


                return Json(new { productDTO });
            }
            catch (Exception ex)
            {
                string exMsg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                return Content(HttpStatusCode.InternalServerError, "loading product details failed!\n error:" + exMsg);
            }
        }
    }
}
