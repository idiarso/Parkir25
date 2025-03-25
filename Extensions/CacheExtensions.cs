using Microsoft.Extensions.Caching.Memory;

namespace ParkIRC.Extensions
{
    public static class CacheExtensions
    {
        public static async Task<T> GetOrCreateAsync<T>(
            this IMemoryCache cache,
            string key,
            Func<Task<T>> factory,
            TimeSpan? slidingExpiration = null,
            TimeSpan? absoluteExpiration = null)
        {
            if (cache.TryGetValue(key, out T result))
            {
                return result;
            }

            result = await factory();

            var options = new MemoryCacheEntryOptions();
            if (slidingExpiration.HasValue)
            {
                options.SlidingExpiration = slidingExpiration.Value;
            }
            if (absoluteExpiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = absoluteExpiration.Value;
            }

            cache.Set(key, result, options);
            return result;
        }
    }
} 