using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record ResetPasswordCommand(string Email, string Otp, string NewPassword) : IRequest<bool>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;
    private readonly ICacheService _cacheService;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IIdentityUnitOfWork uow,
        ICacheService cacheService,
        IPasswordHasher passwordHasher)
    {
        _uow = uow;
        _cacheService = cacheService;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var cacheKey = $"pwd_reset_otp:{request.Email.ToLower().Trim()}";
        var cachedOtp = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);

        if (string.IsNullOrEmpty(cachedOtp) || cachedOtp != request.Otp.Trim())
        {
            throw new ArgumentException("Mã xác thực OTP không chính xác hoặc đã hết hạn (hiệu lực 5 phút).");
        }

        var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy thông tin tài khoản người dùng.");

        if (user.Status != UserStatus.Active)
        {
            throw new InvalidOperationException("Tài khoản đang bị khóa hoặc ngưng hoạt động.");
        }

        // Xóa ngay OTP khỏi cache để chống dùng lại
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);

        // Hash mật khẩu mới và cập nhật
        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await _uow.Users.UpdateAsync(user, cancellationToken);

        // Thu hồi toàn bộ phiên đăng nhập cũ để bảo mật
        await _uow.UserSessions.RevokeAllSessionsByUserIdAsync(user.Id, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}
