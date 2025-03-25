using System;
using System.Linq;

namespace ParkIRC.Web.Extensions
{
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Gets the TimeSpan as a TimeOfDay equivalent
        /// </summary>
        /// <param name="timeSpan">The TimeSpan to convert</param>
        /// <returns>The same TimeSpan value, used to mimic DateTime.TimeOfDay compatibility</returns>
        public static TimeSpan TimeOfDay(this TimeSpan timeSpan)
        {
            return timeSpan;
        }
        
        /// <summary>
        /// Gets the total hours for a nullable TimeSpan
        /// </summary>
        /// <param name="timeSpan">The nullable TimeSpan</param>
        /// <returns>The total hours or 0 if null</returns>
        public static decimal TotalHours(this TimeSpan? timeSpan)
        {
            if (!timeSpan.HasValue)
                return 0;

            return (decimal)Math.Ceiling(timeSpan.Value.TotalHours);
        }

        /// <summary>
        /// Converts a TimeSpan to a human-readable string
        /// </summary>
        /// <param name="timeSpan">The nullable TimeSpan</param>
        /// <returns>A formatted string representing the duration</returns>
        public static string ToReadableString(this TimeSpan? timeSpan)
        {
            if (!timeSpan.HasValue)
                return "0 minutes";

            var ts = timeSpan.Value;
            var parts = new System.Collections.Generic.List<string>();

            if (ts.Days > 0)
                parts.Add($"{ts.Days} day{(ts.Days == 1 ? "" : "s")}");
            if (ts.Hours > 0)
                parts.Add($"{ts.Hours} hour{(ts.Hours == 1 ? "" : "s")}");
            if (ts.Minutes > 0)
                parts.Add($"{ts.Minutes} minute{(ts.Minutes == 1 ? "" : "s")}");

            return string.Join(", ", parts);
        }

        /// <summary>
        /// Converts a TimeSpan to a short string format
        /// </summary>
        /// <param name="timeSpan">The nullable TimeSpan</param>
        /// <returns>A short formatted string representing the duration</returns>
        public static string ToShortString(this TimeSpan? timeSpan)
        {
            if (!timeSpan.HasValue)
                return "0h";

            var ts = timeSpan.Value;
            if (ts.TotalHours >= 1)
                return $"{Math.Ceiling(ts.TotalHours)}h";
            return $"{ts.Minutes}m";
        }

        /// <summary>
        /// Converts a TimeSpan to a string in HH:mm format
        /// </summary>
        /// <param name="timeSpan">The TimeSpan to convert</param>
        /// <returns>A string in HH:mm format</returns>
        public static string ToTimeString(this TimeSpan timeSpan)
        {
            return timeSpan.ToString(@"hh\:mm");
        }

        /// <summary>
        /// Converts a nullable TimeSpan to a string in HH:mm format
        /// </summary>
        /// <param name="timeSpan">The nullable TimeSpan to convert</param>
        /// <returns>A string in HH:mm format or "-" if null</returns>
        public static string ToTimeString(this TimeSpan? timeSpan)
        {
            return timeSpan?.ToString(@"hh\:mm") ?? "00:00";
        }
    }
} 