using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.Commands;

public record LockUserCommand(Guid UserId, string? Reason) : IRequest<bool>;

public class LockUserCommandHandler : IRequestHandler<LockUserCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;

    public LockUserCommandHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(LockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        user.Status = UserStatus.Inactive;
        user.LockReason = request.Reason;
        user.LockedAt = DateTimeOffset.UtcNow;

        await _uow.Users.UpdateAsync(user, cancellationToken);

        // Hủy toàn bộ phiên đăng nhập của người dùng bị khóa ngay lập tức
        await _uow.UserSessions.RevokeAllSessionsByUserIdAsync(user.Id, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}

public record UnlockUserCommand(Guid UserId) : IRequest<bool>;

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
