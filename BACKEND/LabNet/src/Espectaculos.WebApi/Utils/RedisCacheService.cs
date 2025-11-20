using Espectaculos.Application.Abstractions;
using StackExchange.Redis;
using System.Text.Json;
using System.Diagnostics.Metrics;

namespace Espectaculos.WebApi.Utils;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _mux;
    private readonly IDatabase _db;
    private readonly Counter<long> _hits;
    private readonly Counter<long> _misses;
    private readonly ISubscriber _sub;

    public RedisCacheService(IConnectionMultiplexer mux, Meter meter, Counter<long> hits, Counter<long> misses)
    {
        _mux = mux;
        _db = _mux.GetDatabase();
        _sub = _mux.GetSubscriber();
        _hits = hits;
        _misses = misses;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var val = await _db.StringGetAsync(key);
        if (val.HasValue)
        {
            _hits?.Add(1);
            return JsonSerializer.Deserialize<T>(val!);
        }

        _misses?.Add(1);
        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, ttl);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan ttl, CancellationToken ct = default)
    {
        var existing = await GetAsync<T>(key, ct);
        if (existing is not null)
            return existing;

        var created = await factory(ct);
        // si factory devuelve null y quieres cachearlo, decide política aquí; suponemos que sí cacheamos
        await SetAsync(key, created, ttl, ct);
        return created;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        return _db.KeyDeleteAsync(key);
    }

    public Task PublishInvalidationAsync(string channel, string message, CancellationToken ct = default)
    {
        return _sub.PublishAsync(channel, message);
    }
}
