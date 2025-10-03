using DnDAgency.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace DnDAgency.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var data = await _cache.GetStringAsync(key);
        return data == null ? default : JsonSerializer.Deserialize<T>(data);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
            options.AbsoluteExpirationRelativeToNow = expiration;

        var serialized = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, serialized, options);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        // Redis не поддерживает удаление по префиксу напрямую через IDistributedCache
        // Для учебного проекта оставим заглушку, позже можем доработать
        await Task.CompletedTask;
    }
}