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

## Usage by .NET Version

This package is optimized for all modern .NET versions, including **.NET 6 (LTS)**, **.NET 8 (LTS)**, and **.NET 10 (Preview)**.

### 1. Registration (Program.cs / Minimal API)
All versions starting from .NET 6 use the same clean registration syntax in `Program.cs`.

```csharp
using BigInt.AzureRedisCache.MiddleWare;

var builder = <Application>.CreateBuilder(args);

// Register the Azure Redis Cache Service
builder.Services.AddAzureRedisCache(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MyApp"; // Optional prefix for all keys
    options.DefaultExpiry = TimeSpan.FromHours(24);
    options.ThrowOnError = false; // Prevents app crashes if Redis is down
});
```

### 2. Inject and Use

#### **.NET 8 & .NET 10 Style (Primary Constructors)**
If you are using .NET 8 or 10, simplify your code using Primary Constructors.

```csharp
public class MyService(IRedisCacheService cache)
{
    public async Task ProcessDataAsync(string key)
    {
        // Simple Set
        await cache.SetAsync(key, new { Id = 1, Status = "Active" });

        // Simple Get
        var data = await cache.GetAsync<dynamic>(key);
    }
}
```

#### **.NET 6 Style (Standard DI)**
For .NET 6 or older C# versions, use the standard constructor injection.

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
        await _cache.SetAsync("user:123", new { Name = "John" }, TimeSpan.FromHours(1));
        var user = await _cache.GetAsync<dynamic>("user:123");
    }
}
```

#### **Minimal API Usage**
```csharp
app.MapGet("/cache/{id}", async (string id, IRedisCacheService cache) => 
{
    return await cache.GetAsync<object>(id);
});
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
