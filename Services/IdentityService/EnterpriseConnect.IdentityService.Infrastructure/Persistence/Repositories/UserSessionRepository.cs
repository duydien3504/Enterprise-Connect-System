using EnterpriseConnect.BuildingBlocks.Infrastructure.Persistence;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.IdentityService.Infrastructure.Persistence.Repositories;

public class UserSessionRepository : EfRepository<UserSession>, IUserSessionRepository
{
    public UserSessionRepository(IdentityDbContext context) : base(context)
    {
    }

    public async Task<UserSession?> GetActiveByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.TokenHash == tokenHash, cancellationToken);
    }

    public async Task RevokeAllSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var activeSessions = await _dbSet
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var session in activeSessions)
        {
            session.IsRevoked = true;
            session.RevokedAt = now;
        }
    }
}
