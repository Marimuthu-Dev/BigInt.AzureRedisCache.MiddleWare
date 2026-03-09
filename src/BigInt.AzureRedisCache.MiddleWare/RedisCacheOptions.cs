using System;

namespace BigInt.AzureRedisCache.MiddleWare
{
    /// <summary>
    /// Configuration options for the Azure Redis Cache service.
    /// </summary>
    public class RedisCacheOptions
    {
        /// <summary>
        /// Gets or sets the Redis connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the default cache expiry time.
        /// Defaults to 24 hours.
        /// </summary>
        public TimeSpan DefaultExpiry { get; set; } = TimeSpan.FromHours(24);

        /// <summary>
        /// Gets or sets the application-specific key prefix to avoid name collisions in shared Redis instances.
        /// </summary>
        public string InstanceName { get; set; } = string.Empty;

        /// <summary>
        /// Whether to handle exceptions internally or throw them.
        /// </summary>
        public bool ThrowOnError { get; set; } = true;
    }
}
