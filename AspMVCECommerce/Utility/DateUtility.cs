using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace AspMVCECommerce.Utility
{
    public static class DateUtility
    {
        private static List<DateTime> GetWeekDates()
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("ru-RU");
            DateTime startOfWeek = DateTime.Today.AddDays(
           (int)cultureInfo.DateTimeFormat.FirstDayOfWeek -
           (int)DateTime.Today.DayOfWeek);

            var result = Enumerable
              .Range(0, 7)
              .Select(i => startOfWeek
                 .AddDays(i)).ToList();

            return result;
        }

        public static string GetCurrentSunday()
        {
            var dates = GetWeekDates();

            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                return DateTime.Now.ToString("MMM dd, yyyy 23:59:59");
            }
            else
            {
                foreach (var day in dates)
                {
                    if (day.DayOfWeek == DayOfWeek.Sunday)
                    {
                        return day.ToString("MMM dd, yyyy 23:59:59");
                    }
                }
            }
            return null;
        }

    }

}