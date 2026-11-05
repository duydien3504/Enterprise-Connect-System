using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Infrastructure.Persistence.Context;

namespace EnterpriseConnect.RemoteService.Infrastructure.Persistence.Repositories;

public class RemoteUnitOfWork : IRemoteUnitOfWork
{
    private readonly RemoteDbContext _context;
    private readonly IRemoteSessionRepository _remoteSessions;
    private readonly IAccessGrantRepository _accessGrants;

    public RemoteUnitOfWork(
        RemoteDbContext context,
        IRemoteSessionRepository remoteSessions,
        IAccessGrantRepository accessGrants)
    {
        _context = context;
        _remoteSessions = remoteSessions;
        _accessGrants = accessGrants;
    }

    public IRemoteSessionRepository RemoteSessions => _remoteSessions;
    public IAccessGrantRepository AccessGrants => _accessGrants;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
