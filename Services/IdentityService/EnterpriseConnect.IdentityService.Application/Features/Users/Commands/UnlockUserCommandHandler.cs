using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.Commands;

public class UnlockUserCommandHandler : IRequestHandler<UnlockUserCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;

    public UnlockUserCommandHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        user.Status = UserStatus.Active;
        user.LockReason = null;
        user.LockedAt = null;

        await _uow.Users.UpdateAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}
