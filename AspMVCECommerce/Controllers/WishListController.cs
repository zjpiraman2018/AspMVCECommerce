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
                return Json(new { result = "successfully added to wish list!" });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, "add to wish list failed!\n error:" + ex.InnerException.Message);
            }
        }


        [System.Web.Http.HttpPost]
        public IHttpActionResult IsAddedToWishList([FromBody] WishListDTO wishListDTO)
        {
            try
            {
                var wishList = context.WishLists.Where(w=>w.ProductId == wishListDTO.ProductId && w.CustomerId == wishListDTO.CustomerId).SingleOrDefault();

                if (wishList != null)
                {
                    return Json(new { result = "TRUE" });
                }
                else
                {
                    return Json(new { result = "FALSE" });
                }
      
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, "add to wish list failed!\n error:" + ex.InnerException.Message);
            }
        }

    }
}
