using EnterpriseConnect.BuildingBlocks.Application.Audit;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Auth.DTOs;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IIdentityUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IAuditClient? _auditClient;

    public LoginCommandHandler(
        IIdentityUnitOfWork uow,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IAuditClient? auditClient = null)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _auditClient = auditClient;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            RecordAuditFail(request.Email, "Invalid credentials", cancellationToken);
            throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không chính xác.");
        }

        if (user.Status == UserStatus.Inactive || !user.EmailConfirmed || user.Status != UserStatus.Active)
        {
            RecordAuditFail(request.Email, $"Account inactive or unverified ({user.Status})", cancellationToken, user.Id);
            throw new UnauthorizedAccessException($"Tài khoản chưa kích hoạt hoặc bị khóa ({user.Status}).");
        }

        var rolePerms = await _uow.Users.GetRolesAndPermissionsByUserIdAsync(user.Id, cancellationToken);
        var roles = rolePerms.Roles;
        var permissions = rolePerms.Permissions;
        var accessToken = rolePerms.RoleIds != null && rolePerms.RoleIds.Count > 0
            ? _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions, rolePerms.RoleIds)
            : _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        var session = new UserSession
        {
            UserId = user.Id, TokenHash = refreshToken, DeviceType = request.DeviceType,
            DeviceInfo = request.DeviceInfo, IpAddress = request.IpAddress,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7), IsRevoked = false
        };

        await _uow.UserSessions.AddAsync(session, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        _ = _auditClient?.RecordEventAsync(new AuditEventDto
        {
            ServiceName = "IdentityService", ActionName = "AUTH_LOGIN_SUCCESS", TargetEntity = "User",
            TargetId = user.Id.ToString(), ActorId = user.Id, ActorEmail = user.Email,
            ActorRole = roles.FirstOrDefault() ?? "User", RiskLevel = 0,
            IpAddress = request.IpAddress ?? "127.0.0.1", Details = $"Login successful via {request.DeviceType}"
        }, cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken, RefreshToken = refreshToken, ExpiresIn = 3600, TokenType = "Bearer",
            User = new UserSummaryDto { Id = user.Id, Email = user.Email, FullName = user.FullName, Roles = roles, Permissions = permissions }
        };
    }

    private void RecordAuditFail(string email, string reason, CancellationToken ct, Guid? userId = null)
    {
        _ = _auditClient?.RecordEventAsync(new AuditEventDto
        {
            ServiceName = "IdentityService", ActionName = "AUTH_LOGIN_FAILED", TargetEntity = "User",
            TargetId = userId?.ToString(), ActorId = userId ?? Guid.Empty, ActorEmail = email,
            RiskLevel = 2, FailureReason = reason, Details = $"Failed login attempt: {reason}"
        }, ct);
    }
}
