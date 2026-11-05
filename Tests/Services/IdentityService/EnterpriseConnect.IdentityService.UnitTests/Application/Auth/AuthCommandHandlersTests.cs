using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.IdentityService.UnitTests.Application.Auth;

public class AuthCommandHandlersTests
{
    private readonly Mock<IIdentityUnitOfWork> _uowMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUserSessionRepository> _sessionRepoMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtGeneratorMock;

    public AuthCommandHandlersTests()
    {
        _uowMock = new Mock<IIdentityUnitOfWork>();
        _userRepoMock = new Mock<IUserRepository>();
        _sessionRepoMock = new Mock<IUserSessionRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtGeneratorMock = new Mock<IJwtTokenGenerator>();

        _uowMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
        _uowMock.Setup(u => u.UserSessions).Returns(_sessionRepoMock.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokensAndCreateSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "employee@company.com";
        var password = "ValidPassword123@";
        var hashedPassword = "hashed_secret_password";

        var user = new User
        {
            Id = userId,
            Email = email,
            FullName = "John Doe",
            PasswordHash = hashedPassword,
            Status = UserStatus.Active,
            EmailConfirmed = true
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var rolesPermissions = new UserRolePermissionsDto(new List<string> { "Employee" }, new List<string> { "Chat.Read", "Chat.Write" });
        _userRepoMock.Setup(r => r.GetRolesAndPermissionsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rolesPermissions);

        _passwordHasherMock.Setup(p => p.VerifyPassword(password, hashedPassword))
            .Returns(true);

        _jwtGeneratorMock.Setup(j => j.GenerateAccessToken(user, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("mock_jwt_access_token");

        _jwtGeneratorMock.Setup(j => j.GenerateRefreshToken())
            .Returns("mock_refresh_token");

        UserSession? createdSession = null;
        _sessionRepoMock.Setup(s => s.AddAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .Callback<UserSession, CancellationToken>((s, _) => createdSession = s);

        var handler = new LoginCommandHandler(
            _uowMock.Object,
            _passwordHasherMock.Object,
            _jwtGeneratorMock.Object);

        // Act
        var result = await handler.Handle(new LoginCommand(email, password, DeviceType.Web, "Chrome", "127.0.0.1"), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("mock_jwt_access_token");
        result.RefreshToken.Should().Be("mock_refresh_token");
        result.User.Email.Should().Be(email);

        createdSession.Should().NotBeNull();
        createdSession!.UserId.Should().Be(userId);
        createdSession.TokenHash.Should().Be("mock_refresh_token");
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var email = "employee@company.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashed_pass",
            Status = UserStatus.Active
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(p => p.VerifyPassword("wrong_pass", "hashed_pass"))
            .Returns(false);

        var handler = new LoginCommandHandler(
            _uowMock.Object,
            _passwordHasherMock.Object,
            _jwtGeneratorMock.Object);

        // Act
        var act = async () => await handler.Handle(new LoginCommand(email, "wrong_pass", DeviceType.Web, "Web", "127.0.0.1"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*không chính xác*");
    }

    [Fact]
    public async Task Login_WithInactiveUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var email = "inactive@company.com";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashed_pass",
            Status = UserStatus.Inactive
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(p => p.VerifyPassword("any_pass", "hashed_pass"))
            .Returns(true);

        var handler = new LoginCommandHandler(
            _uowMock.Object,
            _passwordHasherMock.Object,
            _jwtGeneratorMock.Object);

        // Act
        var act = async () => await handler.Handle(new LoginCommand(email, "any_pass", DeviceType.Web, "Web", "127.0.0.1"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*bị khóa*");
    }
}
