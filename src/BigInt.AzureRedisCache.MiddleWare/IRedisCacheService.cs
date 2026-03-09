using System;
using System.Threading.Tasks;

namespace BigInt.AzureRedisCache.MiddleWare
{
    /// <summary>
    /// Represents a service for interacting with Azure Redis Cache.
    /// </summary>
    public interface IRedisCacheService
    {
        /// <summary>
        /// Gets a value from the cache.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>The cached value, or default(T) if not found.</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// Sets a value in the cache.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="expiry">Optional expiry time. If not provided, uses configured default.</param>
        /// <returns>A task representing the operation.</returns>
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// Removes a value from the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>True if the key was removed, otherwise false.</returns>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// Checks if a key exists in the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>True if the key exists, otherwise false.</returns>
        Task<bool> ExistsAsync(string key);
    }
}
