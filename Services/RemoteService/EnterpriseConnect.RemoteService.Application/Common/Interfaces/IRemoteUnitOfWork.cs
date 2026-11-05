namespace EnterpriseConnect.RemoteService.Application.Common.Interfaces;

public interface IRemoteUnitOfWork
{
    IRemoteSessionRepository RemoteSessions { get; }
    IAccessGrantRepository AccessGrants { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
