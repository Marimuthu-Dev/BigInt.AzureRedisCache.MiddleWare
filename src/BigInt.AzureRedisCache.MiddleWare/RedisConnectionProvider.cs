using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Sockets;
using System.Threading;

namespace BigInt.AzureRedisCache.MiddleWare
{
    /// <summary>
    /// Manages the Redis ConnectionMultiplexer with self-healing and reconnection logic.
    /// This implementation follows Microsoft's best practices for Azure Redis Cache.
    /// </summary>
    public interface IRedisConnectionProvider : IDisposable
    {
        /// <summary>
        /// Gets the active connection multiplexer.
        /// </summary>
        IConnectionMultiplexer Connection { get; }

        /// <summary>
        /// Forces a reconnection if certain error thresholds are met.
        /// </summary>
        void ForceReconnect();
    }

    /// <summary>
    /// Default implementation of IRedisConnectionProvider with robust reconnection logic.
    /// </summary>
    public class RedisConnectionProvider : IRedisConnectionProvider
    {
        private long _lastReconnectTicks = DateTimeOffset.MinValue.UtcTicks;
        private DateTimeOffset _firstErrorTime = DateTimeOffset.MinValue;
        private DateTimeOffset _previousErrorTime = DateTimeOffset.MinValue;
        private readonly object _reconnectLock = new object();

        private static readonly TimeSpan ReconnectMinFrequency = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan ReconnectErrorThreshold = TimeSpan.FromSeconds(30);

        private Lazy<ConnectionMultiplexer> _lazyConnection;
        private readonly RedisCacheOptions _options;
        private readonly ILogger<RedisConnectionProvider> _logger;

        public RedisConnectionProvider(IOptions<RedisCacheOptions> options, ILogger<RedisConnectionProvider> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lazyConnection = CreateConnection();
        }

        public IConnectionMultiplexer Connection => _lazyConnection.Value;

        private Lazy<ConnectionMultiplexer> CreateConnection()
        {
            return new Lazy<ConnectionMultiplexer>(() =>
            {
                if (string.IsNullOrEmpty(_options.ConnectionString))
                    throw new InvalidOperationException("Redis ConnectionString is not configured.");

                _logger.LogInformation("Creating new Redis connection...");
                return ConnectionMultiplexer.Connect(_options.ConnectionString);
            });
        }

        private void CloseConnection(Lazy<ConnectionMultiplexer> oldConnection)
        {
            if (oldConnection == null || !oldConnection.IsValueCreated) return;

            try
            {
                _logger.LogWarning("Closing old Redis connection...");
                oldConnection.Value.Close();
                oldConnection.Value.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while closing old Redis connection.");
            }
        }

        /// <inheritdoc />
        public void ForceReconnect()
        {
            var utcNow = DateTimeOffset.UtcNow;
            long previousTicks = Interlocked.Read(ref _lastReconnectTicks);
            var previousReconnectTime = new DateTimeOffset(previousTicks, TimeSpan.Zero);
            TimeSpan elapsedSinceLastReconnect = utcNow - previousReconnectTime;

            if (elapsedSinceLastReconnect < ReconnectMinFrequency)
                return;

            lock (_reconnectLock)
            {
                utcNow = DateTimeOffset.UtcNow;
                elapsedSinceLastReconnect = utcNow - previousReconnectTime;

                if (_firstErrorTime == DateTimeOffset.MinValue)
                {
                    _firstErrorTime = utcNow;
                    _previousErrorTime = utcNow;
                    return;
                }

                if (elapsedSinceLastReconnect < ReconnectMinFrequency)
                    return;

                TimeSpan elapsedSinceFirstError = utcNow - _firstErrorTime;
                TimeSpan elapsedSinceMostRecentError = utcNow - _previousErrorTime;

                bool shouldReconnect =
                    elapsedSinceFirstError >= ReconnectErrorThreshold &&
                    elapsedSinceMostRecentError <= ReconnectErrorThreshold;

                _previousErrorTime = utcNow;

                if (!shouldReconnect) return;

                _firstErrorTime = DateTimeOffset.MinValue;
                _previousErrorTime = DateTimeOffset.MinValue;

                Lazy<ConnectionMultiplexer> oldConnection = _lazyConnection;
                CloseConnection(oldConnection);
                _lazyConnection = CreateConnection();
                Interlocked.Exchange(ref _lastReconnectTicks, utcNow.UtcTicks);
                _logger.LogCritical("Redis Force Reconnect triggered and executed.");
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            CloseConnection(_lazyConnection);
        }
    }
}
