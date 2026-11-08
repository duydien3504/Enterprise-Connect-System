using System.Text.Json;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EnterpriseConnect.IdentityService.Infrastructure.Services;

public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<DistributedCacheService> _logger;

    public DistributedCacheService(
        IDistributedCache cache,
        IMemoryCache memoryCache,
        ILogger<DistributedCacheService> logger)
    {
        _cache = cache;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var exp = expiration ?? TimeSpan.FromMinutes(5);
        var json = JsonSerializer.Serialize(value);

        // Always update fast in-memory cache as immediate fallback
        _memoryCache.Set(key, json, exp);

        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = exp
            };
            await _cache.SetStringAsync(key, json, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[DistributedCacheService] Redis SetAsync failed for key '{Key}'. In-memory cache fallback retained.", key);
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        string? json = null;
        try
        {
            json = await _cache.GetStringAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[DistributedCacheService] Redis GetAsync failed for key '{Key}'. Attempting in-memory fallback.", key);
        }

        if (string.IsNullOrEmpty(json))
        {
            if (_memoryCache.TryGetValue(key, out string? memJson) && !string.IsNullOrEmpty(memJson))
            {
                json = memJson;
            }
        }

        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DistributedCacheService] Deserialization failed for key '{Key}'.", key);
            return default;
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[DistributedCacheService] Redis RemoveAsync failed for key '{Key}'. In-memory cache cleared.", key);
        }
    }
}
