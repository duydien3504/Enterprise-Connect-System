using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.IdentityService.UnitTests.Application.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IIdentityUnitOfWork> _uowMock = new();
    private readonly Mock<IUserSessionRepository> _sessionRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtGeneratorMock = new();

    public RefreshTokenCommandHandlerTests()
    {
        _uowMock.Setup(u => u.UserSessions).Returns(_sessionRepoMock.Object);
        _uowMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ValidSession_ShouldPreserveRoleIdsInAccessToken()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var token = "valid_refresh_token";

        var session = new UserSession
        {
            UserId = userId,
            TokenHash = token,
            IsRevoked = false,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1)
        };
        var user = new User { Id = userId, Status = UserStatus.Active, Email = "test@corp.com" };
        var rolePerms = new UserRolePermissionsDto(new[] { "Admin" }, new[] { "Read" }) { RoleIds = new[] { roleId } };

        _sessionRepoMock.Setup(s => s.GetActiveByTokenHashAsync(token, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _userRepoMock.Setup(u => u.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepoMock.Setup(u => u.GetRolesAndPermissionsByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(rolePerms);
        _jwtGeneratorMock.Setup(j => j.GenerateAccessToken(user, rolePerms.Roles, rolePerms.Permissions, rolePerms.RoleIds)).Returns("new_jwt_with_roles");
        _jwtGeneratorMock.Setup(j => j.GenerateRefreshToken()).Returns("new_refresh_token");

        var handler = new RefreshTokenCommandHandler(_uowMock.Object, _jwtGeneratorMock.Object);
        var result = await handler.Handle(new RefreshTokenCommand(token), CancellationToken.None);

        result.AccessToken.Should().Be("new_jwt_with_roles");
        _jwtGeneratorMock.Verify(j => j.GenerateAccessToken(user, rolePerms.Roles, rolePerms.Permissions, rolePerms.RoleIds), Times.Once);
        _sessionRepoMock.Verify(s => s.AddAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RevokedSession_ShouldTriggerReuseDetection()
    {
        var userId = Guid.NewGuid();
        var token = "stolen_token";
        var session = new UserSession { UserId = userId, TokenHash = token, IsRevoked = true };

        _sessionRepoMock.Setup(s => s.GetActiveByTokenHashAsync(token, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var handler = new RefreshTokenCommandHandler(_uowMock.Object, _jwtGeneratorMock.Object);
        var act = async () => await handler.Handle(new RefreshTokenCommand(token), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*bất thường*");
        _sessionRepoMock.Verify(s => s.RevokeAllSessionsByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExpiredSession_ShouldRevokeAndThrow()
    {
        var token = "expired_token";
        var session = new UserSession { TokenHash = token, IsRevoked = false, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5) };

        _sessionRepoMock.Setup(s => s.GetActiveByTokenHashAsync(token, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var handler = new RefreshTokenCommandHandler(_uowMock.Object, _jwtGeneratorMock.Object);
        var act = async () => await handler.Handle(new RefreshTokenCommand(token), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*hết hạn*");
        session.IsRevoked.Should().BeTrue();
    }
}
