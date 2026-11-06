using System.Security.Cryptography;
using System.Text;
using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EnterpriseConnect.AdminService.Infrastructure.Services;

public class AuditLogDeduplicator : IAuditLogDeduplicator
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan DeduplicationWindow = TimeSpan.FromSeconds(2);

    public AuditLogDeduplicator(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool IsDuplicate(string correlationId, string serviceName, string actionName, Guid actorId, string endpointUrl)
    {
        var rawKey = $"{correlationId}:{serviceName}:{actionName}:{actorId}:{endpointUrl}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        var cacheKey = $"audit_dedup_{Convert.ToHexString(hashBytes)}";

        if (_cache.TryGetValue(cacheKey, out _))
        {
            return true;
        }

        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = DeduplicationWindow
        };

        _cache.Set(cacheKey, true, cacheEntryOptions);
        return false;
    }
}
