using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Infrastructure.Persistence.Context;

namespace EnterpriseConnect.MediaService.Infrastructure.Persistence.Repositories;

public class MediaUnitOfWork : IMediaUnitOfWork
{
    private readonly MediaDbContext _context;
    private readonly ICallSessionRepository _callSessions;

    public MediaUnitOfWork(MediaDbContext context, ICallSessionRepository callSessions)
    {
        _context = context;
        _callSessions = callSessions;
    }

    public ICallSessionRepository CallSessions => _callSessions;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
