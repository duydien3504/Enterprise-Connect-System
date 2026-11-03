using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record RevokeSessionByIdCommand(Guid SessionId, Guid UserId) : IRequest<bool>;

public class RevokeSessionByIdCommandHandler : IRequestHandler<RevokeSessionByIdCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;

    public RevokeSessionByIdCommandHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(RevokeSessionByIdCommand request, CancellationToken cancellationToken)
    {
        var session = await _uow.UserSessions.GetByIdAsync(request.SessionId, cancellationToken);
        if (session != null && session.UserId == request.UserId && !session.IsRevoked)
        {
            session.IsRevoked = true;
            session.RevokedAt = DateTimeOffset.UtcNow;
            await _uow.UserSessions.UpdateAsync(session, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
