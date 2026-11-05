using EnterpriseConnect.RemoteService.Domain.Entities;
using EnterpriseConnect.RemoteService.Domain.Enums;

namespace EnterpriseConnect.RemoteService.Application.Common.Interfaces;

public interface IAccessGrantRepository
{
    Task<AccessGrant?> GetBySessionAndTypeAsync(Guid sessionId, PermissionType permissionType, CancellationToken cancellationToken = default);
    Task AddAsync(AccessGrant grant, CancellationToken cancellationToken = default);
    Task UpdateAsync(AccessGrant grant, CancellationToken cancellationToken = default);
}
