using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System;
using System.Threading.Tasks;

namespace ParkIRC.Extensions
{
    public static class CacheExtensions
    {
        public static T GetOrCreate<T>(this IMemoryCache cache, string key, Func<T> factory, TimeSpan? expiration = null)
        {
            if (!cache.TryGetValue(key, out T value))
            {
                value = factory();

                var cacheOptions = new MemoryCacheEntryOptions();
                if (expiration.HasValue)
                    cacheOptions.SetAbsoluteExpiration(expiration.Value);

                cache.Set(key, value, cacheOptions);
            }

            return value;
        }

        /// <summary>
        /// Gets an item from the cache or creates it if it doesn't exist.
        /// </summary>
        /// <typeparam name="TItem">The type of the item to get or create.</typeparam>
        /// <param name="cache">The cache to get or add the item to.</param>
        /// <param name="key">The key to look up the item with.</param>
        /// <param name="factory">The asynchronous factory function to create the item if it doesn't exist.</param>
        /// <param name="absoluteExpiration">The absolute expiration date for the cache entry.</param>
        /// <param name="slidingExpiration">The sliding expiration for the cache entry.</param>
        /// <returns>The item from the cache or the created item.</returns>
        public static async Task<TItem> GetOrCreateAsync<TItem>(
            this IMemoryCache cache,
            object key,
            Func<Task<TItem>> factory,
            DateTimeOffset? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null)
        {
            // Try to get the item from the cache
            if (cache.TryGetValue(key, out TItem result))
            {
                return result;
            }

            // Item is not in the cache, so create it
            var newItem = await factory();

            // Setup cache options
            var options = new MemoryCacheEntryOptions();
            
            if (absoluteExpiration.HasValue)
            {
                options.AbsoluteExpiration = absoluteExpiration.Value;
            }
            
            if (slidingExpiration.HasValue)
            {
                options.SlidingExpiration = slidingExpiration.Value;
            }

            // Add the item to the cache
            cache.Set(key, newItem, options);

            return newItem;
        }

        public static async Task<T> GetOrCreateAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            var value = await cache.GetAsync<T>(key);
            if (value == null)
            {
                value = await factory();

                var options = new DistributedCacheEntryOptions();
                if (expiration.HasValue)
                    options.SetAbsoluteExpiration(expiration.Value);

                await cache.SetAsync(key, value, options);
            }

            return value;
        }

        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            var bytes = await cache.GetAsync(key);
            if (bytes == null)
                return default;

            var json = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<T>(json);
        }

        public static async Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions? options = null)
        {
            var json = JsonSerializer.Serialize(value);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            await cache.SetAsync(key, bytes, options ?? new DistributedCacheEntryOptions());
        }

        public static async Task<(bool Success, T Value)> TryGetAsync<T>(this IDistributedCache cache, string key)
        {
            try
            {
                var value = await cache.GetAsync<T>(key);
                return (value != null, value);
            }
            catch
            {
                return (false, default);
            }
        }

        public static async Task<bool> TrySetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions? options = null)
        {
            try
            {
                await cache.SetAsync(key, value, options);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<T> GetOrRefreshAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            var value = await cache.GetAsync<T>(key);
            if (value != null)
                return value;

            value = await factory();
            if (value != null)
            {
                var options = new DistributedCacheEntryOptions();
                if (expiration.HasValue)
                    options.SetAbsoluteExpiration(expiration.Value);

                await cache.SetAsync(key, value, options);
            }

            return value;
        }

        public static async Task<bool> RemoveAsync(this IDistributedCache cache, string key)
        {
            try
            {
                await cache.RemoveAsync(key);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> RefreshAsync(this IDistributedCache cache, string key)
        {
            try
            {
                await cache.RefreshAsync(key);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static MemoryCacheEntryOptions WithSlidingExpiration(this MemoryCacheEntryOptions options, TimeSpan expiration)
        {
            return options.SetSlidingExpiration(expiration);
        }

        public static MemoryCacheEntryOptions WithAbsoluteExpiration(this MemoryCacheEntryOptions options, TimeSpan expiration)
        {
            return options.SetAbsoluteExpiration(expiration);
        }

        public static MemoryCacheEntryOptions WithPriority(this MemoryCacheEntryOptions options, CacheItemPriority priority)
        {
            return options.SetPriority(priority);
        }

        public static MemoryCacheEntryOptions WithSize(this MemoryCacheEntryOptions options, long size)
        {
            return options.SetSize(size);
        }

        public static DistributedCacheEntryOptions WithSlidingExpiration(this DistributedCacheEntryOptions options, TimeSpan expiration)
        {
            return options.SetSlidingExpiration(expiration);
        }

        public static DistributedCacheEntryOptions WithAbsoluteExpiration(this DistributedCacheEntryOptions options, TimeSpan expiration)
        {
            return options.SetAbsoluteExpiration(expiration);
        }

        public static DistributedCacheEntryOptions WithAbsoluteExpiration(this DistributedCacheEntryOptions options, DateTimeOffset expiration)
        {
            return options.SetAbsoluteExpiration(expiration);
        }
    }
} 