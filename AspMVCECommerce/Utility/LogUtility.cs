using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using AspMVCECommerce.Models;
using System.Text;

namespace AspMVCECommerce.Utility
{
    public static class LogUtility
    {
        public static void Write(string logType,string logMessage, ApplicationDbContext context)
        {
            try
            {
                var log = new Log();
                log.Message = logMessage;
                log.Type = logType;
                log.Created = DateTime.Now;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("\r\nLog Entry : ");
                sb.AppendLine("\r\nLog Entry : ");
                sb.AppendLine(DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString());
                sb.AppendLine("  :");
                sb.AppendLine("  :" + logMessage);
                sb.AppendLine("-------------------------------");
                log.Message = sb.ToString();

                context.Logs.Add(log);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }


    }

}