using AspMVCECommerce.DTO;
using AspMVCECommerce.Models;
using AspMVCECommerce.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;




namespace AspMVCECommerce.Controllers
{
    public class LineItemController : ApiController
    {
        private ApplicationDbContext context = new ApplicationDbContext();



        [System.Web.Http.HttpPost]
        public IHttpActionResult AddLineItem([FromBody] LineItemDTO lineItemDTO )
        {
            try
            {
                string userId = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
                int shoppingCartId = ShoppingCartUtility.GetShoppingCartId(userId, context);

                var lineItem = lineItemDTO.ToLineItem();
                lineItem.ShoppingCartId = shoppingCartId;


                context.LineItems.Add(lineItem);
                context.SaveChanges();

                return Json(new { result = "successfully added to cart!" });
            }
            catch (Exception ex)
            {
                string exMsg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //return Json(new { result = "add to cart failed!\n error:" + exMsg });
                return Content(HttpStatusCode.InternalServerError, "add to cart failed!\n error:" + exMsg);
            }
        }

        [System.Web.Http.HttpPost]
        public IHttpActionResult CheckOutfuckers([FromBody] LineItemDTO lineItemDTO)
        {
            try
            {
                string userId = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
                // CHECK OUT CODE HERE

                // CREATE NEW SHOPPINGCART RECORD WHEN CHECKOUT IS COMPLETED
                // UNCOMMENT WHEN CODE IS READY
                // AddShoppingCart(userId);
                return Json(new { result = "successfully checkout cart!" });
            }
            catch (Exception ex)
            {
                string exMsg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                //return Json(new { result = "add to cart failed!\n error:" + exMsg });
                return Content(HttpStatusCode.InternalServerError, "checkout cart failed!\n error:" + exMsg);
            }
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult DeleteLineItem(int lineItemId)
        {
            try
            {
                var lineItem = context.LineItems.Find(lineItemId);
                context.LineItems.Remove(lineItem);
                context.SaveChanges();

                return Json(new { result = "successfully remove item!" });
            }
            catch (Exception ex)
            {
                string exMsg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                return Content(HttpStatusCode.InternalServerError, "remove lineitem failed!\n error:" + exMsg);
            }
        }


        [System.Web.Http.HttpGet]
        public IHttpActionResult GetLineItemsOfCurrentUser()
        {
            try
            {
                string userId = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
                int shoppingCartId = ShoppingCartUtility.GetShoppingCartId(userId, context);

                var lineItems = context.LineItems.Include(l => l.Product).Include(l => l.Product.Images).Include(l => l.Size).Include(l => l.Color).Where(l => l.ShoppingCartId == shoppingCartId).ToList();
                var lineItemViewDTOList = new List<LineItemViewDTO>();

                foreach (var item in lineItems)
                {
                    var lineItemView = new LineItemViewDTO();
                    lineItemView.ImagePath = item.Product.Images.Where(i => i.Default == true).FirstOrDefault().ImagePath;
                    lineItemView.ColorName = item.Color == null ? "DEFAULT" : item.Color.Name;
                    lineItemView.ColorId = item.ColorId;
                    lineItemView.SizeId = item.SizeId;
                    lineItemView.SizeName = item.Size == null ? "DEFAULT" : item.Size.Name;
                    lineItemView.LineItemId = item.LineItemId;
                    lineItemView.ProductId = item.ProductId;
                    lineItemView.Quantity = item.Quantity;
                    lineItemView.TotalPrice = item.TotalPrice;
                    lineItemView.ProductName = item.Product.Name;
                    lineItemViewDTOList.Add(lineItemView);
                }


                return Json(new { lineItemViewDTOList });
            }
            catch (Exception ex)
            {
                string exMsg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                return Content(HttpStatusCode.InternalServerError, "remove lineitem failed!\n error:" + exMsg);
            }
        }
    }
}
