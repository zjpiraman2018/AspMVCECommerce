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
    public class NewsLetterController : ApiController
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        [System.Web.Http.HttpPost]
        public IHttpActionResult AddNewsLetter([FromBody] NewsLetterDTO newsLetterDTO)
        {
            try
            {
                if (!IsDuplicate(newsLetterDTO.Email))
                {
                    context.NewsLetters.Add(newsLetterDTO.ToNewsLetter());
                    context.SaveChanges();
                    return Json(new { result = "successfully added news letter!" });
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, "newsletter already exist!");
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, "add new newsletter failed!\n error:" + ex.InnerException.Message);
            }
        }

        private bool IsDuplicate(string email)
        {
            return context.NewsLetters.Where(n => n.Email.Trim().ToUpper() == email.Trim().ToUpper()).Count() > 0 ? true : false;
        }
    }
}
