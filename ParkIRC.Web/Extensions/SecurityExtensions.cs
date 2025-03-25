using System.Security.Cryptography;
using System.Text;

namespace ParkIRC.Extensions
{
    public static class SecurityExtensions
    {
        public static string ToMD5(this string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string ToSHA256(this string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string ToSHA512(this string input)
        {
            using (var sha512 = SHA512.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha512.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string ToBase64(this string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes);
        }

        public static string FromBase64(this string input)
        {
            var inputBytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(inputBytes);
        }

        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateSecureToken(int length = 32)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[length];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes)
                    .Replace("/", "_")
                    .Replace("+", "-")
                    .TrimEnd('=');
            }
        }

        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPassword(this string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            var hasNumber = password.Any(char.IsDigit);
            var hasUpperChar = password.Any(char.IsUpper);
            var hasLowerChar = password.Any(char.IsLower);
            var hasSpecialChar = password.Any(ch => !char.IsLetterOrDigit(ch));
            var hasMinLength = password.Length >= 8;

            return hasNumber && hasUpperChar && hasLowerChar && hasSpecialChar && hasMinLength;
        }

        public static string SanitizeHtml(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return input.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&#x27;")
                       .Replace("/", "&#x2F;");
        }

        public static string SanitizeFileName(this string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return fileName;

            var invalidChars = Path.GetInvalidFileNameChars();
            return new string(fileName.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
        }

        public static string SanitizeUrl(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            return Uri.EscapeDataString(url);
        }
    }
} 