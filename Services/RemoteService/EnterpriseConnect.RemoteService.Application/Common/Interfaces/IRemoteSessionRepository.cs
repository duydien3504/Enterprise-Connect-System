using EnterpriseConnect.RemoteService.Domain.Entities;
using EnterpriseConnect.RemoteService.Domain.Enums;

namespace EnterpriseConnect.RemoteService.Application.Common.Interfaces;

public interface IRemoteSessionRepository
{
    Task<RemoteSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RemoteSession?> GetByIdWithGrantsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RemoteSession?> GetByCodeWithGrantsAsync(string code, CancellationToken cancellationToken = default);
    Task<RemoteSession?> GetActiveSessionByRequesterAsync(Guid requesterId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RemoteSession>> GetUserHistoryAsync(Guid userId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RemoteSession>> GetExpiredSessionsAsync(DateTimeOffset threshold, CancellationToken cancellationToken = default);
    Task AddAsync(RemoteSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(RemoteSession session, CancellationToken cancellationToken = default);
}
