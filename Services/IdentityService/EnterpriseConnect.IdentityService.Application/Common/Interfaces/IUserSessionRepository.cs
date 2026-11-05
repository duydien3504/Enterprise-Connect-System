using EnterpriseConnect.BuildingBlocks.Application.Contracts;
using EnterpriseConnect.IdentityService.Domain.Entities;

namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public interface IUserSessionRepository : IRepository<UserSession>
{
    Task<UserSession?> GetActiveByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task RevokeAllSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
