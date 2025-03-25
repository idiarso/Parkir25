using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ParkIRC.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetClientIP(this HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].ToString();
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress?.ToString();
            }
            return ip;
        }

        public static string GetUserAgent(this HttpContext context)
        {
            return context.Request.Headers["User-Agent"].ToString();
        }

        public static string GetReferer(this HttpContext context)
        {
            return context.Request.Headers["Referer"].ToString();
        }

        public static bool IsAjaxRequest(this HttpContext context)
        {
            return context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        public static async Task<T> GetSessionValueAsync<T>(this HttpContext context, string key)
        {
            var value = context.Session.GetString(key);
            if (string.IsNullOrEmpty(value))
                return default;

            return JsonSerializer.Deserialize<T>(value);
        }

        public static async Task SetSessionValueAsync<T>(this HttpContext context, string key, T value)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            context.Session.SetString(key, serializedValue);
        }

        public static async Task RemoveSessionValueAsync(this HttpContext context, string key)
        {
            context.Session.Remove(key);
        }

        public static async Task ClearSessionAsync(this HttpContext context)
        {
            context.Session.Clear();
        }

        public static string GetCurrentUrl(this HttpContext context)
        {
            return $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
        }

        public static string GetBaseUrl(this HttpContext context)
        {
            return $"{context.Request.Scheme}://{context.Request.Host}";
        }

        public static bool IsAuthenticated(this HttpContext context)
        {
            return context.User?.Identity?.IsAuthenticated ?? false;
        }

        public static string GetUserId(this HttpContext context)
        {
            return context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        public static string GetUserName(this HttpContext context)
        {
            return context.User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        }

        public static IEnumerable<string> GetUserRoles(this HttpContext context)
        {
            return context.User?.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();
        }

        public static bool IsInRole(this HttpContext context, string role)
        {
            return context.User?.IsInRole(role) ?? false;
        }

        public static bool HasClaim(this HttpContext context, string type, string value)
        {
            return context.User?.HasClaim(type, value) ?? false;
        }

        public static string GetClaimValue(this HttpContext context, string type)
        {
            return context.User?.FindFirst(type)?.Value;
        }

        public static async Task SetTempDataAsync<T>(this HttpContext context, string key, T value)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            context.Session.SetString($"TempData_{key}", serializedValue);
        }

        public static async Task<T> GetTempDataAsync<T>(this HttpContext context, string key)
        {
            var value = context.Session.GetString($"TempData_{key}");
            if (string.IsNullOrEmpty(value))
                return default;

            context.Session.Remove($"TempData_{key}");
            return JsonSerializer.Deserialize<T>(value);
        }

        public static async Task<bool> HasTempDataAsync(this HttpContext context, string key)
        {
            return context.Session.GetString($"TempData_{key}") != null;
        }

        public static async Task RemoveTempDataAsync(this HttpContext context, string key)
        {
            context.Session.Remove($"TempData_{key}");
        }

        public static async Task ClearTempDataAsync(this HttpContext context)
        {
            var keys = context.Session.Keys.Where(k => k.StartsWith("TempData_")).ToList();
            foreach (var key in keys)
            {
                context.Session.Remove(key);
            }
        }
    }
} 