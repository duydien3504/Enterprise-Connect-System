using System.Collections.Concurrent;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Enums;

namespace EnterpriseConnect.IdentityService.Infrastructure.Services;

public class MemoryPresenceService : IPresenceService
{
    private class PresenceEntry
    {
        public PresenceStatus Status { get; set; }
        public DeviceType DeviceType { get; set; }
        public DateTimeOffset LastHeartbeatAt { get; set; }
    }

    private static readonly ConcurrentDictionary<Guid, PresenceEntry> _presenceCache = new();
    private static readonly TimeSpan _heartbeatTimeout = TimeSpan.FromMinutes(2);

    public Task SetPresenceAsync(Guid userId, PresenceStatus status, DeviceType deviceType, CancellationToken cancellationToken = default)
    {
        _presenceCache.AddOrUpdate(
            userId,
            _ => new PresenceEntry
            {
                Status = status,
                DeviceType = deviceType,
                LastHeartbeatAt = DateTimeOffset.UtcNow
            },
            (_, existing) =>
            {
                existing.Status = status;
                existing.DeviceType = deviceType;
                existing.LastHeartbeatAt = DateTimeOffset.UtcNow;
                return existing;
            });

        return Task.CompletedTask;
    }

    public Task<PresenceStatus> GetPresenceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (_presenceCache.TryGetValue(userId, out var entry))
        {
            if (DateTimeOffset.UtcNow - entry.LastHeartbeatAt <= _heartbeatTimeout)
            {
                return Task.FromResult(entry.Status);
            }
        }

        return Task.FromResult(PresenceStatus.Offline);
    }

    public Task<Dictionary<Guid, PresenceStatus>> GetBatchPresenceAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<Guid, PresenceStatus>();
        var now = DateTimeOffset.UtcNow;

        foreach (var userId in userIds)
        {
            if (_presenceCache.TryGetValue(userId, out var entry) && (now - entry.LastHeartbeatAt <= _heartbeatTimeout))
            {
                result[userId] = entry.Status;
            }
            else
            {
                result[userId] = PresenceStatus.Offline;
            }
        }

        return Task.FromResult(result);
    }
}
