using AspMVCECommerce.DTO;
using AspMVCECommerce.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;

namespace AspMVCECommerce.Controllers
{
    public class NewsLetterController : ApiController
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        private static string Combine(string uri1, string uri2)
        {
            uri1 = uri1.TrimEnd('/');
            uri2 = uri2.TrimStart('/');
            return string.Format("{0}/{1}", uri1, uri2);
        }

        [System.Web.Http.HttpPost]
        public IHttpActionResult AddNewsLetter([FromBody] NewsLetterDTO newsLetterDTO)
        {
            try
            {
                if (!IsDuplicate(newsLetterDTO.Email))
                {
                    var newsLetter = newsLetterDTO.ToNewsLetter();
                    context.NewsLetters.Add(newsLetter);
                    context.SaveChanges();

                    string baseUrl = ConfigurationManager.AppSettings["BaseUrl"];

                    var callbackUrl = Combine(baseUrl, "/Home/ConfirmEmail?id=" + newsLetter.NewsLetterId.ToString());
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account",
                    // "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");  
                    string body = string.Empty;
                    string path = System.Web.Hosting.HostingEnvironment.MapPath("~/EmailTemplate/AccountConfirmation.html");

                    using (StreamReader reader = new StreamReader(path))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{ConfirmationLink}", callbackUrl);
                    body = body.Replace("{UserName}", newsLetterDTO.Email);


                    string HostAddress = ConfigurationManager.AppSettings["Host"].ToString();
                    string FormEmailId = ConfigurationManager.AppSettings["MailFrom"].ToString();
                    string Password = ConfigurationManager.AppSettings["Password"].ToString();
                    string Port = ConfigurationManager.AppSettings["Port"].ToString();
                    MailMessage mailMessage = new MailMessage();
                   
                    mailMessage.From = new MailAddress(FormEmailId,"Zaldy's Ecommerce");
                    mailMessage.Subject = "Confirm your account";
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;
                    mailMessage.To.Add(new MailAddress(newsLetterDTO.Email));
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = HostAddress;
                    smtp.EnableSsl = true;
                    NetworkCredential networkCredential = new NetworkCredential();
                    networkCredential.UserName = mailMessage.From.Address;
                    networkCredential.Password = Password;
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = networkCredential;
                    smtp.Port = Convert.ToInt32(Port);
                    smtp.Send(mailMessage);
                    // Email successfully send if code pass here


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
