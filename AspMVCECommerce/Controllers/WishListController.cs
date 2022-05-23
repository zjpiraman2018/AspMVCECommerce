using AspMVCECommerce.DTO;
using AspMVCECommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AspMVCECommerce.Controllers
{
    public class WishListController : ApiController
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        [System.Web.Http.HttpPost]
        public IHttpActionResult AddWishList([FromBody] WishListDTO wishListDTO)
        {
            try 
            { 
                context.WishLists.Add(wishListDTO.ToWishList());
                context.SaveChanges();
                return Json(new { result = "successfully added to wish list!", count = context.WishLists.Where(w => w.CustomerId == wishListDTO.CustomerId).Count().ToString() });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, "add to wish list failed!\n error:" + ex.Message);
            }
        }

        [System.Web.Http.HttpPost]
        public IHttpActionResult RemoveWishList([FromBody] WishListDTO wishListDTO)
        {
            try
            {
                var wishList = context.WishLists.Where(w => w.CustomerId == wishListDTO.CustomerId && w.ProductId == wishListDTO.ProductId).FirstOrDefault();
                context.WishLists.Remove(wishList);
                context.SaveChanges();
                return Json(new { result = "successfully removed to wish list!", count = context.WishLists.Where(w => w.CustomerId == wishListDTO.CustomerId).Count().ToString() });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, "remove to wish list failed!\n error:" + ex.Message);
            }
        }


    }
}
