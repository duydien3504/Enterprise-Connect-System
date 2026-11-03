using EnterpriseConnect.IdentityService.Domain.Enums;

namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public interface IPresenceService
{
    Task SetPresenceAsync(Guid userId, PresenceStatus status, DeviceType deviceType, CancellationToken cancellationToken = default);
    Task<PresenceStatus> GetPresenceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, PresenceStatus>> GetBatchPresenceAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}
