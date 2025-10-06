using DnDAgency.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Polly;
using Polly.CircuitBreaker;

namespace DnDAgency.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;
    private static readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

    static CacheService()
    {
        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 1,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (ex, duration) => { },
                onReset: () => { },
                onHalfOpen: () => { }
            );
    }

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                var data = await _cache.GetStringAsync(key);
                return data == null ? default : JsonSerializer.Deserialize<T>(data);
            });
        }
        catch (BrokenCircuitException)
        {
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Cache unavailable: {Message}", ex.Message);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                var options = new DistributedCacheEntryOptions();
                if (expiration.HasValue)
                    options.AbsoluteExpirationRelativeToNow = expiration;

                var serialized = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, serialized, options);
            });
        }
        catch (BrokenCircuitException) { }
        catch { }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                await _cache.RemoveAsync(key);
            });
        }
        catch { }
    }

    public Task RemoveByPrefixAsync(string prefix) => Task.CompletedTask;
}