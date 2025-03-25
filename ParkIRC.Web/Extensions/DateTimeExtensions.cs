using System.Globalization;

namespace ParkIRC.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToFormattedString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string ToShortFormattedString(this DateTime dateTime)
        {
            return dateTime.ToString("MM/dd/yyyy");
        }

        public static string ToTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("HH:mm:ss");
        }

        public static string ToDateString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        public static string ToMonthYearString(this DateTime dateTime)
        {
            return dateTime.ToString("MMMM yyyy");
        }

        public static string ToDayMonthYearString(this DateTime dateTime)
        {
            return dateTime.ToString("dd MMMM yyyy");
        }

        public static DateTime StartOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, date.Kind);
        }

        public static DateTime EndOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999, date.Kind);
        }

        public static DateTime StartOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).StartOfDay();
        }

        public static DateTime EndOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            return date.StartOfWeek(startOfWeek).AddDays(6).EndOfDay();
        }

        public static DateTime StartOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1, 0, 0, 0, date.Kind);
        }

        public static DateTime EndOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59, 999, date.Kind);
        }

        public static DateTime StartOfYear(this DateTime date)
        {
            return new DateTime(date.Year, 1, 1, 0, 0, 0, date.Kind);
        }

        public static DateTime EndOfYear(this DateTime date)
        {
            return new DateTime(date.Year, 12, 31, 23, 59, 59, 999, date.Kind);
        }

        public static DateTime StartOfQuarter(this DateTime date)
        {
            var month = date.Month;
            return new DateTime(date.Year, month - ((month - 1) % 3), 1, 0, 0, 0, date.Kind);
        }

        public static DateTime EndOfQuarter(this DateTime date)
        {
            return date.StartOfQuarter().AddMonths(3).AddDays(-1).EndOfDay();
        }

        public static bool IsWeekend(this DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsWeekday(this DateTime date)
        {
            return !date.IsWeekend();
        }

        public static int Age(this DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age))
                age--;
            return age;
        }

        public static string ToRelativeTime(this DateTime date)
        {
            var timeSince = DateTime.Now.Subtract(date);

            if (timeSince.TotalMilliseconds < 0)
                return "in the future";

            if (timeSince.TotalSeconds < 1)
                return "just now";

            if (timeSince.TotalSeconds < 60)
                return $"{timeSince.Seconds} seconds ago";

            if (timeSince.TotalMinutes < 60)
                return timeSince.Minutes == 1 ? "a minute ago" : $"{timeSince.Minutes} minutes ago";

            if (timeSince.TotalHours < 24)
                return timeSince.Hours == 1 ? "an hour ago" : $"{timeSince.Hours} hours ago";

            if (timeSince.TotalDays < 7)
                return timeSince.Days == 1 ? "yesterday" : $"{timeSince.Days} days ago";

            if (timeSince.TotalDays < 30)
            {
                var weeks = Math.Floor(timeSince.TotalDays / 7);
                return weeks == 1 ? "a week ago" : $"{weeks} weeks ago";
            }

            if (timeSince.TotalDays < 365)
            {
                var months = Math.Floor(timeSince.TotalDays / 30);
                return months == 1 ? "a month ago" : $"{months} months ago";
            }

            var years = Math.Floor(timeSince.TotalDays / 365);
            return years == 1 ? "a year ago" : $"{years} years ago";
        }

        public static bool IsBetween(this DateTime date, DateTime start, DateTime end)
        {
            return date >= start && date <= end;
        }

        public static DateTime NextWorkday(this DateTime date)
        {
            var next = date.AddDays(1);
            while (next.IsWeekend())
            {
                next = next.AddDays(1);
            }
            return next;
        }

        public static DateTime PreviousWorkday(this DateTime date)
        {
            var previous = date.AddDays(-1);
            while (previous.IsWeekend())
            {
                previous = previous.AddDays(-1);
            }
            return previous;
        }

        public static int GetQuarter(this DateTime date)
        {
            return (date.Month - 1) / 3 + 1;
        }

        public static bool IsLastDayOfMonth(this DateTime date)
        {
            return date.Day == DateTime.DaysInMonth(date.Year, date.Month);
        }

        public static bool IsFirstDayOfMonth(this DateTime date)
        {
            return date.Day == 1;
        }

        public static string ToFriendlyDateString(this DateTime date)
        {
            if (date.Date == DateTime.Today)
                return "Today";
            if (date.Date == DateTime.Today.AddDays(-1))
                return "Yesterday";
            if (date.Date == DateTime.Today.AddDays(1))
                return "Tomorrow";

            return date.ToString("MMMM d, yyyy");
        }

        public static string ToFriendlyTimeString(this DateTime date)
        {
            return date.ToString("h:mm tt").ToLower();
        }

        public static string ToFriendlyDateTimeString(this DateTime date)
        {
            return $"{date.ToFriendlyDateString()} at {date.ToFriendlyTimeString()}";
        }

        public static DateTime SetTime(this DateTime date, int hour, int minute = 0, int second = 0, int millisecond = 0)
        {
            return new DateTime(date.Year, date.Month, date.Day, hour, minute, second, millisecond, date.Kind);
        }

        public static DateTime ToLocalTime(this DateTime date, string timeZoneId)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTime(date, timeZone);
        }

        public static bool IsSameDay(this DateTime date1, DateTime date2)
        {
            return date1.Date == date2.Date;
        }

        public static bool IsSameMonth(this DateTime date1, DateTime date2)
        {
            return date1.Year == date2.Year && date1.Month == date2.Month;
        }

        public static bool IsSameYear(this DateTime date1, DateTime date2)
        {
            return date1.Year == date2.Year;
        }

        public static int DaysInMonth(this DateTime date)
        {
            return DateTime.DaysInMonth(date.Year, date.Month);
        }

        public static DateTime LastDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.DaysInMonth(), 23, 59, 59, 999, date.Kind);
        }

        public static DateTime FirstDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1, 0, 0, 0, date.Kind);
        }

        public static int WeekOfYear(this DateTime date)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                date,
                CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
                CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
        }
    }
} 