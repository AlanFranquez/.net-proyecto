using Espectaculos.Application.Abstractions;

namespace Espectaculos.WebApi.Utils;

public class NullCacheService : ICacheService
{
    private readonly ILogger<NullCacheService> _logger;

    public NullCacheService(ILogger<NullCacheService> logger)
    {
        _logger = logger;
    }
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        _logger.LogDebug("[NullCache] GET {Key} → MISS (cache disabled)", key);
        return Task.FromResult(default(T));
    }
    
    public Task<T?> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan ttl, CancellationToken ct = default)
    {
        _logger.LogDebug("[NullCache] GET {Key} → MISS (cache disabled)", key);
        return Task.FromResult(default(T));
    }
    
    public Task PublishInvalidationAsync(string key, string message, CancellationToken ct = default)
    {
        _logger.LogDebug("[NullCache] GET {Key} → MISS (cache disabled)", key);
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        _logger.LogDebug("[NullCache] SET {Key} (ignored, cache disabled)", key);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _logger.LogDebug("[NullCache] REMOVE {Key} (ignored, cache disabled)", key);
        return Task.CompletedTask;
    }
}
