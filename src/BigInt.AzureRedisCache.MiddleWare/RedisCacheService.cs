using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace BigInt.AzureRedisCache.MiddleWare
{
    /// <summary>
    /// A robust, production-ready implementation of IRedisCacheService using StackExchange.Redis and System.Text.Json.
    /// Includes self-healing logic and automatic reconnection for Azure Redis environments.
    /// </summary>
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IRedisConnectionProvider _connectionProvider;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly RedisCacheOptions _options;
        private const int MaxRetryAttempts = 3;

        /// <summary>
        /// Initializes a new instance of the RedisCacheService.
        /// </summary>
        /// <param name="connectionProvider">The connection provider managing the singleton connection multiplexer.</param>
        /// <param name="options">Options for the cache service.</param>
        /// <param name="logger">Logger instance.</param>
        public RedisCacheService(
            IRedisConnectionProvider connectionProvider,
            IOptions<RedisCacheOptions> options,
            ILogger<RedisCacheService> logger)
        {
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private IDatabase Database => _connectionProvider.Connection.GetDatabase();

        private string GetFullKey(string key) => string.IsNullOrEmpty(_options.InstanceName) ? key : $"{_options.InstanceName}:{key}";

        private async Task<T> ExecuteWithRetryAsync<T>(Func<IDatabase, Task<T>> action)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    return await action(Database).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is RedisConnectionException || ex is SocketException)
                {
                    attempts++;
                    _logger.LogWarning(ex, "Redis connection error on attempt {Attempt}. Key: {Key}", attempts, "Unknown");
                    
                    _connectionProvider.ForceReconnect();

                    if (attempts >= MaxRetryAttempts)
                    {
                        if (_options.ThrowOnError) throw;
                        return default;
                    }
                }
                catch (ObjectDisposedException)
                {
                    attempts++;
                    if (attempts >= MaxRetryAttempts)
                    {
                        if (_options.ThrowOnError) throw;
                        return default;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during Redis operation.");
                    if (_options.ThrowOnError) throw;
                    return default;
                }
            }
        }

        /// <inheritdoc />
        public Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            return ExecuteWithRetryAsync(async db =>
            {
                var fullKey = GetFullKey(key);
                var value = await db.StringGetAsync(fullKey).ConfigureAwait(false);

                if (value.IsNull)
                {
                    _logger.LogInformation("Cache miss for key: {Key}", fullKey);
                    return default;
                }

                return JsonSerializer.Deserialize<T>(value);
            });
        }

        /// <inheritdoc />
        public Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));

            return ExecuteWithRetryAsync(async db =>
            {
                var fullKey = GetFullKey(key);
                var jsonValue = JsonSerializer.Serialize(value);
                var effectiveExpiry = expiry ?? _options.DefaultExpiry;

                var result = await db.StringSetAsync(fullKey, jsonValue, effectiveExpiry).ConfigureAwait(false);
                
                if (result)
                {
                    _logger.LogDebug("Cache set for key: {Key} with expiry: {Expiry}", fullKey, effectiveExpiry);
                }
                return result;
            });
        }

        /// <inheritdoc />
        public Task<bool> RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            return ExecuteWithRetryAsync(async db =>
            {
                var fullKey = GetFullKey(key);
                return await db.KeyDeleteAsync(fullKey).ConfigureAwait(false);
            });
        }

        /// <inheritdoc />
        public Task<bool> ExistsAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            return ExecuteWithRetryAsync(async db =>
            {
                var fullKey = GetFullKey(key);
                return await db.KeyExistsAsync(fullKey).ConfigureAwait(false);
            });
        }
    }
}
