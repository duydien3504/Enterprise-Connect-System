namespace EnterpriseConnect.MediaService.Application.Common.Interfaces;

public interface IMediaUnitOfWork
{
    ICallSessionRepository CallSessions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
