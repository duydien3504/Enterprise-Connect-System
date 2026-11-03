using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Auth.DTOs;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record LoginCommand(
    string Email,
    string Password,
    DeviceType DeviceType = DeviceType.Web,
    string? DeviceInfo = null,
    string? IpAddress = null
) : IRequest<AuthResponseDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IIdentityUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IIdentityUnitOfWork uow,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không chính xác.");

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không chính xác.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException($"Tài khoản hiện đang bị khóa hoặc ngưng hoạt động ({user.Status}).");
        }

        // Truy vấn phân quyền nhanh qua phương thức chuyên biệt trong repository
        var (roles, permissions) = await _uow.Users.GetRolesAndPermissionsByUserIdAsync(user.Id, cancellationToken);

        // Generate tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // Tạo UserSession mới
        var session = new UserSession
        {
            UserId = user.Id,
            TokenHash = refreshToken,
            DeviceType = request.DeviceType,
            DeviceInfo = request.DeviceInfo,
            IpAddress = request.IpAddress,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            IsRevoked = false
        };

        await _uow.UserSessions.AddAsync(session, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600,
            TokenType = "Bearer",
            User = new UserSummaryDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                DepartmentId = user.DepartmentId,
                Roles = roles,
                Permissions = permissions
            }
        };
    }
}
