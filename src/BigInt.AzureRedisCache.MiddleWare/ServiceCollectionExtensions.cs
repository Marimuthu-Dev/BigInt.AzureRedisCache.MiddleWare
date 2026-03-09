using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace BigInt.AzureRedisCache.MiddleWare
{
    /// <summary>
    /// Service collection extensions for registering the Azure Redis Cache service.
    /// </summary>
    public static class RedisCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Azure Redis Cache service to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="setupAction">The configuration action.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddAzureRedisCache(this IServiceCollection services, Action<RedisCacheOptions> setupAction)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction));

            services.Configure(setupAction);

            // Register the Connection Provider (Singleton)
            // It manages the ConnectionMultiplexer handles reconnection.
            services.TryAddSingleton<IRedisConnectionProvider, RedisConnectionProvider>();

            // Register the Cache Service (Singleton)
            // It provides the high-level API for cache operations.
            services.TryAddSingleton<IRedisCacheService, RedisCacheService>();

            return services;
        }
    }
}
