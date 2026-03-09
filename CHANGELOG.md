# Changelog

All notable changes to this project will be documented in this file.

## [2.0.0] - 2026-03-09

### Added
- Created `IRedisCacheService` interface for better abstraction and testability.
- Added `RedisCacheService` implementation with full async support.
- Implemented `IRedisConnectionProvider` with robust reconnection and self-healing logic (Microsoft best practices).
- Introduced `RedisCacheOptions` for strongly-typed configuration.
- Added `ServiceCollectionExtensions` for easy integration with `IServiceCollection`.
- Integrated `ILogger` for better observability and diagnostics.
- Added XML documentation generation for all public members.

### Changed (Breaking Changes)
- **Refactored Architecture**: Migrated from static wrapper to Dependency Injection (DI) pattern.
- **Serialization**: Switched from `BinaryFormatter` (deprecated/insecure) to `System.Text.Json`.
- **Async Only**: All cache operations are now async-first to align with modern .NET standards.
- **Configuration**: Replaced `ConfigurationManager` dependency with modern `IOptions` pattern.
- **NuGet Packaging**: Moved metadata from `.nuspec` to `.csproj`.

### Removed
- Removed static `CacheManager` class.
- Removed static `RedisCache` class.
- Removed dependency on `System.Configuration.ConfigurationManager`.
- Removed `.nuspec` file (now managed via `.csproj`).
