using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IIdentityUnitOfWork uow,
        IPasswordHasher passwordHasher)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new ArgumentException("Mật khẩu hiện tại không chính xác.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await _uow.Users.UpdateAsync(user, cancellationToken);

        // Security Hook: Thu hồi toàn bộ session trên mọi thiết bị
        await _uow.UserSessions.RevokeAllSessionsByUserIdAsync(user.Id, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}
