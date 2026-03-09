# BigInt.AzureRedisCache.MiddleWare

[![NuGet](https://img.shields.io/nuget/v/BigInt.AzureRedisCache.MiddleWare.svg)](https://www.nuget.org/packages/BigInt.AzureRedisCache.MiddleWare)

A modern, high-performance, and resilient Azure Redis Cache library for .NET applications. This package simplifies Redis integration by providing a robust service-based API with full Dependency Injection support and automatic self-healing.

## Key Features

- **Production-Ready**: Implements Microsoft's best practices for `ConnectionMultiplexer` management.
- **Resilient**: Automatic "Force Reconnect" logic for handling transient Azure Redis connection issues.
- **Async-First**: All operations are asynchronous to prevent thread-blocking.
- **Clean API**: Simple `IRedisCacheService` with `GetAsync<T>`, `SetAsync<T>`, and `RemoveAsync`.
- **Modern Serialization**: Uses `System.Text.Json` (v8+) for high-performance object serialization.
- **DI Friendly**: Easy registration via `AddAzureRedisCache` extension method.
- **Observable**: Full integration with `ILogger` for better diagnostic support.

## Installation

```bash
dotnet add package BigInt.AzureRedisCache.MiddleWare
```

## Quick Start

### 1. Register the Service

In your `Program.cs` or `Startup.cs`:

```csharp
using BigInt.AzureRedisCache.MiddleWare;

builder.Services.AddAzureRedisCache(options =>
{
    options.ConnectionString = "YOUR_REDIS_CONNECTION_STRING";
    options.InstanceName = "MyApp"; // Optional prefix for all keys
});
```

### 2. Inject and Use

```csharp
public class MyService
{
    private readonly IRedisCacheService _cache;

    public MyService(IRedisCacheService cache)
    {
        _cache = cache;
    }

    public async Task DoWorkAsync()
    {
        // Set value
        await _cache.SetAsync("user:123", new User { Name = "John" }, TimeSpan.FromHours(1));

        // Get value
        var user = await _cache.GetAsync<User>("user:123");
    }
}
```

## Configuration Options

| Option | Type | Default | Description |
| --- | --- | --- | --- |
| `ConnectionString` | `string` | `null` | **Required**. Your Redis connection string. |
| `InstanceName` | `string` | `""` | Optional prefix for all keys (e.g. `Dev:`, `Prod:`). |
| `DefaultExpiry` | `TimeSpan` | 24 Hours | Default cache duration for objects. |
| `ThrowOnError` | `bool` | `true` | Whether to propagate Redis exceptions or fail gracefully. |

## Resilience and Fault Tolerance

This library handles connection pooling and self-healing. If Redis becomes temporarily unavailable or if a connection timeout occurs in Azure, the library will:
1. Log the error via `ILogger`.
2. Attempt automatic retries for transient failures.
3. Automatically re-create the `ConnectionMultiplexer` if errors persist (Force Reconnect pattern).

---
Developed by **Marimuthu Kandasamy** (BigInt Technical Systems)
