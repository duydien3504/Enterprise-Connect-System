using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Auth.DTOs;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress = null,
    string? DeviceInfo = null
) : IRequest<AuthResponseDto>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IIdentityUnitOfWork _uow;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(
        IIdentityUnitOfWork uow,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _uow = uow;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingSession = await _uow.UserSessions.GetActiveByTokenHashAsync(request.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedAccessException("Phiên đăng nhập không tồn tại hoặc không hợp lệ.");

        // Reuse detection: Token đã bị revoke trước đó -> Nghi ngờ rò rỉ token!
        if (existingSession.IsRevoked)
        {
            await _uow.UserSessions.RevokeAllSessionsByUserIdAsync(existingSession.UserId, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedAccessException("Phát hiện phiên làm việc bất thường đã bị thu hồi. Toàn bộ thiết bị đã được đăng xuất để bảo vệ tài khoản.");
        }

        if (existingSession.ExpiresAt < DateTimeOffset.UtcNow)
        {
            existingSession.IsRevoked = true;
            existingSession.RevokedAt = DateTimeOffset.UtcNow;
            await _uow.UserSessions.UpdateAsync(existingSession, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedAccessException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
        }

        var user = await _uow.Users.GetByIdAsync(existingSession.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Người dùng không tồn tại.");

        if (user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException("Tài khoản người dùng đã bị khóa hoặc ngừng hoạt động.");
        }

        // Lấy quyền hạn mới nhất
        var (roles, permissions) = await _uow.Users.GetRolesAndPermissionsByUserIdAsync(user.Id, cancellationToken);

        // Rotate Refresh Token
        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        existingSession.IsRevoked = true;
        existingSession.RevokedAt = DateTimeOffset.UtcNow;
        existingSession.ReplacedByTokenHash = newRefreshToken;
        await _uow.UserSessions.UpdateAsync(existingSession, cancellationToken);

        // Tạo phiên mới
        var newSession = new UserSession
        {
            UserId = user.Id,
            TokenHash = newRefreshToken,
            DeviceType = existingSession.DeviceType,
            DeviceInfo = request.DeviceInfo ?? existingSession.DeviceInfo,
            IpAddress = request.IpAddress ?? existingSession.IpAddress,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            IsRevoked = false
        };

        await _uow.UserSessions.AddAsync(newSession, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
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
