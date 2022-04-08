using AspMVCECommerce.DTO;
using AspMVCECommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Http;

namespace AspMVCECommerce.Controllers
{
    public class ReviewController : ApiController
    {

        private ApplicationDbContext context = new ApplicationDbContext();

        [System.Web.Http.HttpPost]
        public IHttpActionResult LoadData(int productId)
        {

            var Request = new HttpRequestWrapper(HttpContext.Current.Request);
            // get Start (paging start index) and length (page size for paging)
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            //Get sort columns valu
            //var sortColumn = Request.Form.GetValues("columns[" +
            //    Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();

            //var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;


            // get search string
            //string searchStr = Request.Form.GetValues("search[value]").FirstOrDefault();


            var v = (from a in context.Reviews
                     where a.ProductId == productId
                     select a);



            //if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            //{
            //    v = v.OrderBy(sortColumn + " " + sortColumnDir);
            //}


            totalRecords = v.Count();

            var data = v.OrderBy(a => a.ReviewId).Skip(skip).Take(pageSize).ToList();
            var dataDTO = new List<ReviewDTO>();

            foreach (var item in data)
            {
                dataDTO.Add(item.ToReviewDTO());
            }



            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = dataDTO });
        }

        [System.Web.Http.HttpGet]
        public IHttpActionResult GetReviewRatingInfo(int productId)
        {

            var reviewRatings = context.Reviews.Where(review => review.ProductId == productId).GroupBy(n => n.Rating)
                         .Select(n => new
                         {
                             rating = n.Key,
                             count = n.Count()
                         })
                         .OrderBy(n => n.rating).ToList();

            var ratingsWithRecord = reviewRatings.Select(rev => (int)rev.rating).ToList();


            for (int i = 0; i < 5; i++)
            {
                if (!ratingsWithRecord.Contains(i + 1))
                {
                    reviewRatings.Add(new { rating = (double)i + 1, count = 0 });
                }
            }

            reviewRatings = reviewRatings.OrderBy(rev => rev.rating).ToList();



            int totalReviews = reviewRatings.Sum(x => x.count);
            int a = reviewRatings[0].count, b = reviewRatings[1].count, c = reviewRatings[2].count, d = reviewRatings[3].count, e = reviewRatings[4].count, r = totalReviews;
            double AR =  (double)(1 * a + 2 * b + 3 * c + 4 * d + 5 * e) / r;


            var completeReviewRatings = reviewRatings.Select(rev => new { rating = rev.rating, count = rev.count, percent = ((double)rev.count / totalReviews).ToString("0.00") }).ToList();



            return Json(new { data = completeReviewRatings, averageRating = AR.ToAverageRatingString(), totalReviews = totalReviews });
        }


        [System.Web.Http.HttpPost]
        public IHttpActionResult AddReview([FromBody] ReviewDTO reviewDTO)
        {
            try
            {
                context.Reviews.Add(reviewDTO.ToReview());
                context.SaveChanges();
                return Json(new { result = "successfully added reviews!" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "add new reviews failed!\n error:" + ex.InnerException.Message });
            }
        }

    }
}
