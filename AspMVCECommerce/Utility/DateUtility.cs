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

        public static DateTime? GetCurrentSunday()
        {
            var dates = GetWeekDates();


            foreach (var day in dates)
            {
                if(day.DayOfWeek == DayOfWeek.Sunday)
                {
                    return day;
                }
            }
     
            return null;
        }

    }

}