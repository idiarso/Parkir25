using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ParkIRC.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static string ToTitleCase(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        public static string ToCamelCase(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            var words = value.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            var result = words[0].ToLower();
            for (int i = 1; i < words.Length; i++)
            {
                result += char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
            return result;
        }

        public static string ToSnakeCase(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }

        public static string ToKebabCase(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x.ToString() : x.ToString())).ToLower();
        }

        public static string RemoveSpecialCharacters(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            return Regex.Replace(value, "[^a-zA-Z0-9]+", "", RegexOptions.Compiled);
        }

        public static string RemoveAccents(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            var normalizedString = value.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string Left(this string value, int length)
        {
            if (value.IsNullOrEmpty())
                return value;

            length = Math.Abs(length);
            return value.Length <= length ? value : value.Substring(0, length);
        }

        public static string Right(this string value, int length)
        {
            if (value.IsNullOrEmpty())
                return value;

            length = Math.Abs(length);
            return value.Length <= length ? value : value.Substring(value.Length - length);
        }

        public static string Truncate(this string value, int maxLength, string suffix = "...")
        {
            if (value.IsNullOrEmpty() || value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength - suffix.Length) + suffix;
        }

        public static bool IsValidEmail(this string value)
        {
            if (value.IsNullOrEmpty())
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(value);
                return addr.Address == value;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidUrl(this string value)
        {
            if (value.IsNullOrEmpty())
                return false;

            return Uri.TryCreate(value, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static bool IsValidIpAddress(this string value)
        {
            if (value.IsNullOrEmpty())
                return false;

            return System.Net.IPAddress.TryParse(value, out _);
        }

        public static bool IsNumeric(this string value)
        {
            if (value.IsNullOrEmpty())
                return false;

            return value.All(char.IsDigit);
        }

        public static bool IsAlpha(this string value)
        {
            if (value.IsNullOrEmpty())
                return false;

            return value.All(char.IsLetter);
        }

        public static bool IsAlphaNumeric(this string value)
        {
            if (value.IsNullOrEmpty())
                return false;

            return value.All(c => char.IsLetterOrDigit(c));
        }

        public static string ReplaceFirst(this string value, string search, string replace)
        {
            if (value.IsNullOrEmpty())
                return value;

            int pos = value.IndexOf(search);
            if (pos < 0)
                return value;

            return value.Substring(0, pos) + replace + value.Substring(pos + search.Length);
        }

        public static string ReplaceLast(this string value, string search, string replace)
        {
            if (value.IsNullOrEmpty())
                return value;

            int pos = value.LastIndexOf(search);
            if (pos < 0)
                return value;

            return value.Substring(0, pos) + replace + value.Substring(pos + search.Length);
        }

        public static string RemoveHtmlTags(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            return Regex.Replace(value, "<.*?>", string.Empty);
        }

        public static string StripHtml(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            return Regex.Replace(value, "<.*?>|&.*?;", string.Empty);
        }

        public static string ToSlug(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            value = value.ToLower();
            value = value.RemoveAccents();
            value = Regex.Replace(value, @"[^a-z0-9\s-]", "");
            value = Regex.Replace(value, @"\s+", " ").Trim();
            value = Regex.Replace(value, @"\s", "-");
            return value;
        }

        public static string ToBase64(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            var bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        public static string FromBase64(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            var bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string Reverse(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            var charArray = value.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static int ToInt(this string value, int defaultValue = 0)
        {
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        public static decimal ToDecimal(this string value, decimal defaultValue = 0)
        {
            return decimal.TryParse(value, out var result) ? result : defaultValue;
        }

        public static double ToDouble(this string value, double defaultValue = 0)
        {
            return double.TryParse(value, out var result) ? result : defaultValue;
        }

        public static bool ToBool(this string value, bool defaultValue = false)
        {
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }

        public static DateTime? ToDateTime(this string value)
        {
            return DateTime.TryParse(value, out var result) ? result : null;
        }

        public static string TruncateWithEllipsis(this string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
        }

        public static string ToSafeFileName(this string fileName)
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        }
    }
} 