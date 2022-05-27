using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Reflection;

namespace AspMVCECommerce.Utility
{
    public static class LogUtility
    {
        private static string m_exePath = string.Empty;

        public static void Write(string logMessage)
        {
            m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            using (StreamWriter w = File.AppendText(m_exePath + "\\" + "Error_Log.txt"))
            {
                Log(logMessage, w);
            }
        }

        private static void Log(string logMessage, TextWriter txtWriter)
        {
            txtWriter.Write("\r\nLog Entry : ");
            txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            txtWriter.WriteLine("  :");
            txtWriter.WriteLine("  :{0}", logMessage);
            txtWriter.WriteLine("-------------------------------");
        }
    }

}