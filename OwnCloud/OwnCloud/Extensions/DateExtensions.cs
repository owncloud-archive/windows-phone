using System;

namespace OwnCloud.Extensions
{
    public static class DateExtensions
    {
        public static DateTime FirstOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, 0, dt.Kind);
        }

        public static DateTime LastOfMonth(this DateTime dt)
        {
            return dt.FirstOfMonth().AddMonths(1).AddDays(-1);
        }

        public static DateTime FirstDayOfWeek(this DateTime dt)
        {
            int delta = DayOfWeek.Monday - dt.DayOfWeek;
            return dt.AddDays(delta);
        }

        public static DateTime LastDayOfWeek(this DateTime dt)
        {
            int delta = DayOfWeek.Sunday - dt.DayOfWeek;
            return dt.AddDays(delta + (delta < 0 ? 7 : 0));
        }

        public static int GetMonthCount(this DateTime dt)
        {
            var first = dt.FirstOfMonth().FirstDayOfWeek();
            var last = dt.LastOfMonth().LastDayOfWeek();

            int i = 0;

            do
            {
                i++;
                first = first.AddDays(7);
            }
            while (first < last);

            return i;
        }

        public static int CountWeekInMonths(int year, int month, DayOfWeek wkstart)
        {
            DateTime first = new DateTime(year, month, 1);
            int firstwkday = (int)first.DayOfWeek;

            int otherwkday = (int)wkstart;
            int offset = ((otherwkday + 7) - firstwkday) % 7;

            double weeks = (double)(DateTime.DaysInMonth(year, month) - offset) / 7d;
            return (int)Math.Ceiling(weeks);
        }

        public static DateTime CombineWithTime(this DateTime date, DateTime time)
        {
            return new DateTime(date.Year,date.Month,date.Day,time.Hour,time.Minute, time.Second,time.Minute,date.Date.Kind);
        }
    }
}
