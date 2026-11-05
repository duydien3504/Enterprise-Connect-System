using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public class LogoutAllDevicesCommandHandler : IRequestHandler<LogoutAllDevicesCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;

    public LogoutAllDevicesCommandHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(LogoutAllDevicesCommand request, CancellationToken cancellationToken)
    {
        await _uow.UserSessions.RevokeAllSessionsByUserIdAsync(request.UserId, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}
