using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Domain.Entities;
using EnterpriseConnect.RemoteService.Domain.Enums;
using EnterpriseConnect.RemoteService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.RemoteService.Infrastructure.Persistence.Repositories;

public class AccessGrantRepository : IAccessGrantRepository
{
    private readonly RemoteDbContext _context;

    public AccessGrantRepository(RemoteDbContext context)
    {
        _context = context;
    }

    public async Task<AccessGrant?> GetBySessionAndTypeAsync(
        Guid sessionId, PermissionType permissionType, CancellationToken cancellationToken = default)
    {
        return await _context.AccessGrants
            .FirstOrDefaultAsync(g => g.SessionId == sessionId && g.PermissionType == permissionType, cancellationToken);
    }

    public async Task AddAsync(AccessGrant grant, CancellationToken cancellationToken = default)
    {
        await _context.AccessGrants.AddAsync(grant, cancellationToken);
    }

    public Task UpdateAsync(AccessGrant grant, CancellationToken cancellationToken = default)
    {
        _context.AccessGrants.Update(grant);
        return Task.CompletedTask;
    }
}
