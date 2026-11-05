using System.Security.Cryptography;
using System.Text;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IIdentityUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICacheService _cacheService;
    private readonly IEmailSender _emailSender;
    private readonly IPublicUrlProvider _publicUrlProvider;

    public RegisterUserCommandHandler(
        IIdentityUnitOfWork uow,
        IPasswordHasher passwordHasher,
        ICacheService cacheService,
        IEmailSender emailSender,
        IPublicUrlProvider publicUrlProvider)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _cacheService = cacheService;
        _emailSender = emailSender;
        _publicUrlProvider = publicUrlProvider;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (existing != null)
        {
            throw new ArgumentException("Email này đã được sử dụng bởi một tài khoản khác.");
        }

        var user = new User
        {
            Email = request.Email.Trim().ToLower(),
            FullName = request.FullName.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            DepartmentId = request.DepartmentId,
            Position = request.Position,
            Phone = request.Phone,
            Status = UserStatus.Inactive,
            EmailConfirmed = false
        };

        await _uow.Users.AddAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        // Sinh token kích hoạt email ngẫu nhiên 32 bytes
        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        // Lưu vào cache 24h
        await _cacheService.SetAsync($"email_verify:{tokenHash}", user.Id.ToString(), TimeSpan.FromHours(24), cancellationToken);

        // Gửi link kích hoạt (đi qua API Gateway)
        var verifyLink = $"{_publicUrlProvider.IdentityBaseUrl}/api/Auth/verify-email?token={rawToken}";
        var bodyHtml = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                <h2>Xác thực tài khoản Enterprise Connect</h2>
                <p>Chào <strong>{user.FullName}</strong>,</p>
                <p>Vui lòng nhấn vào liên kết bên dưới để xác thực địa chỉ email và kích hoạt tài khoản của bạn:</p>
                <p style='margin: 20px 0;'><a href='{verifyLink}' style='background: #007bff; color: #fff; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Kích hoạt tài khoản</a></p>
                <p>Liên kết này có hiệu lực trong vòng 24 giờ.</p>
            </div>";

        await _emailSender.SendEmailAsync(user.Email, "[Enterprise Connect] Xác thực email tài khoản", bodyHtml, cancellationToken);
        return user.Id;
    }
}
