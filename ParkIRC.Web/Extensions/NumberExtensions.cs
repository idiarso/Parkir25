using System.Globalization;

namespace ParkIRC.Extensions
{
    public static class NumberExtensions
    {
        public static string ToOrdinal(this int number)
        {
            var lastDigit = number % 10;
            var lastTwoDigits = number % 100;

            if (lastTwoDigits is 11 or 12 or 13)
                return $"{number}th";

            return lastDigit switch
            {
                1 => $"{number}st",
                2 => $"{number}nd",
                3 => $"{number}rd",
                _ => $"{number}th"
            };
        }

        public static string ToWords(this int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + ToWords(Math.Abs(number));

            var words = "";

            if ((number / 1000000) > 0)
            {
                words += ToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += ToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += ToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words.Trim();
        }

        public static string ToCurrency(this decimal amount, string currencyCode = "USD", bool useSymbol = true)
        {
            var culture = CultureInfo.GetCultureInfo("en-US");
            var format = useSymbol ? "C" : "N2";
            
            return amount.ToString(format, culture);
        }

        public static string ToPercentage(this decimal value, int decimals = 2)
        {
            return value.ToString($"P{decimals}");
        }

        public static decimal RoundToNearest(this decimal value, decimal nearest)
        {
            return Math.Round(value / nearest) * nearest;
        }

        public static decimal RoundUp(this decimal value, int decimals)
        {
            var multiplier = (decimal)Math.Pow(10, decimals);
            return Math.Ceiling(value * multiplier) / multiplier;
        }

        public static decimal RoundDown(this decimal value, int decimals)
        {
            var multiplier = (decimal)Math.Pow(10, decimals);
            return Math.Floor(value * multiplier) / multiplier;
        }

        public static bool IsEven(this int value)
        {
            return value % 2 == 0;
        }

        public static bool IsOdd(this int value)
        {
            return value % 2 != 0;
        }

        public static bool IsPrime(this int value)
        {
            if (value <= 1)
                return false;
            if (value == 2)
                return true;
            if (value % 2 == 0)
                return false;

            var boundary = (int)Math.Floor(Math.Sqrt(value));

            for (int i = 3; i <= boundary; i += 2)
            {
                if (value % i == 0)
                    return false;
            }

            return true;
        }

        public static bool IsInRange(this int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        public static bool IsInRange(this decimal value, decimal min, decimal max)
        {
            return value >= min && value <= max;
        }

        public static bool IsInRange(this double value, double min, double max)
        {
            return value >= min && value <= max;
        }

        public static int Clamp(this int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        public static decimal Clamp(this decimal value, decimal min, decimal max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        public static double Clamp(this double value, double min, double max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        public static string ToFileSize(this long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double len = bytes;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        public static TimeSpan Hours(this int value)
        {
            return TimeSpan.FromHours(value);
        }

        public static TimeSpan Minutes(this int value)
        {
            return TimeSpan.FromMinutes(value);
        }

        public static TimeSpan Seconds(this int value)
        {
            return TimeSpan.FromSeconds(value);
        }

        public static TimeSpan Milliseconds(this int value)
        {
            return TimeSpan.FromMilliseconds(value);
        }

        public static TimeSpan Days(this int value)
        {
            return TimeSpan.FromDays(value);
        }

        public static string ToRoman(this int number)
        {
            if (number < 1 || number > 3999)
                throw new ArgumentOutOfRangeException(nameof(number), "Value must be between 1 and 3999");

            string[] romanThousands = { "", "M", "MM", "MMM" };
            string[] romanHundreds = { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };
            string[] romanTens = { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };
            string[] romanOnes = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };

            return romanThousands[number / 1000] +
                   romanHundreds[(number % 1000) / 100] +
                   romanTens[(number % 100) / 10] +
                   romanOnes[number % 10];
        }

        public static string ToHex(this int value)
        {
            return value.ToString("X");
        }

        public static string ToBinary(this int value)
        {
            return Convert.ToString(value, 2);
        }

        public static int FromBinary(this string binary)
        {
            return Convert.ToInt32(binary, 2);
        }

        public static int FromHex(this string hex)
        {
            return Convert.ToInt32(hex, 16);
        }
    }
} 