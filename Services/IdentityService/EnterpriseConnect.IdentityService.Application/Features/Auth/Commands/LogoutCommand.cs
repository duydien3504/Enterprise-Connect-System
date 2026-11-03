using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record LogoutCommand(string RefreshToken) : IRequest<bool>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;

    public LogoutCommandHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var session = await _uow.UserSessions.GetActiveByTokenHashAsync(request.RefreshToken, cancellationToken);
        if (session != null && !session.IsRevoked)
        {
            session.IsRevoked = true;
            session.RevokedAt = DateTimeOffset.UtcNow;
            await _uow.UserSessions.UpdateAsync(session, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
