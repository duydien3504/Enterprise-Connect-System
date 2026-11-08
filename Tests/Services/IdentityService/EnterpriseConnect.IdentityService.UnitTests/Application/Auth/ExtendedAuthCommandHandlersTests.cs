using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.IdentityService.UnitTests.Application.Auth;

public class ExtendedAuthCommandHandlersTests
{
    private readonly Mock<IIdentityUnitOfWork> _uowMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUserSessionRepository> _sessionRepoMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IEmailSender> _emailSenderMock;

    public ExtendedAuthCommandHandlersTests()
    {
        _uowMock = new Mock<IIdentityUnitOfWork>();
        _userRepoMock = new Mock<IUserRepository>();
        _sessionRepoMock = new Mock<IUserSessionRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _cacheServiceMock = new Mock<ICacheService>();
        _emailSenderMock = new Mock<IEmailSender>();

        _uowMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
        _uowMock.Setup(u => u.UserSessions).Returns(_sessionRepoMock.Object);
    }

    [Fact]
    public async Task ForgotPassword_WhenUserExists_ShouldGenerateOtpSaveToCacheAndSendEmail()
    {
        // Arrange
        var email = "test@company.com";
        var user = new User { Id = Guid.NewGuid(), Email = email, Status = UserStatus.Active };

        _userRepoMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        string? savedKey = null;
        string? savedOtp = null;
        _cacheServiceMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, TimeSpan?, CancellationToken>((k, v, _, _) =>
            {
                savedKey = k;
                savedOtp = v;
            });

        var emailSentTcs = new TaskCompletionSource<bool>();
        _emailSenderMock
            .Setup(e => e.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => emailSentTcs.TrySetResult(true))
            .Returns(Task.CompletedTask);

        var handler = new ForgotPasswordCommandHandler(_uowMock.Object, _cacheServiceMock.Object, _emailSenderMock.Object);

        // Act
        var result = await handler.Handle(new ForgotPasswordCommand(email), CancellationToken.None);
        await Task.WhenAny(emailSentTcs.Task, Task.Delay(500));

        // Assert
        result.Should().BeTrue();
        savedKey.Should().Be($"pwd_reset_otp:{email}");
        savedOtp.Should().HaveLength(6);
        _emailSenderMock.Verify(e => e.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_WhenOtpValid_ShouldUpdatePasswordHashAndRevokeOldSessions()
    {
        // Arrange
        var email = "test@company.com";
        var otp = "654321";
        var newPassword = "NewSecretPassword123@";
        var user = new User { Id = Guid.NewGuid(), Email = email, Status = UserStatus.Active, PasswordHash = "old_hash" };

        _cacheServiceMock.Setup(c => c.GetAsync<string>($"pwd_reset_otp:{email}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(otp);

        _userRepoMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(p => p.HashPassword(newPassword))
            .Returns("new_hash");

        var handler = new ResetPasswordCommandHandler(_uowMock.Object, _cacheServiceMock.Object, _passwordHasherMock.Object);

        // Act
        var result = await handler.Handle(new ResetPasswordCommand(email, otp, newPassword), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.PasswordHash.Should().Be("new_hash");
        _cacheServiceMock.Verify(c => c.RemoveAsync($"pwd_reset_otp:{email}", It.IsAny<CancellationToken>()), Times.Once);
        _sessionRepoMock.Verify(s => s.RevokeAllSessionsByUserIdAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_WhenOtpInvalidOrExpired_ShouldThrowArgumentException()
    {
        // Arrange
        var email = "test@company.com";
        _cacheServiceMock.Setup(c => c.GetAsync<string>($"pwd_reset_otp:{email}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null); // Hết hạn

        var handler = new ResetPasswordCommandHandler(_uowMock.Object, _cacheServiceMock.Object, _passwordHasherMock.Object);

        // Act
        var act = async () => await handler.Handle(new ResetPasswordCommand(email, "111111", "NewPass"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*hết hạn*");
    }

    [Fact]
    public async Task RevokeSessionById_WhenSessionBelongsToUser_ShouldSetRevoked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var session = new UserSession
        {
            Id = sessionId,
            UserId = userId,
            IsRevoked = false
        };

        _sessionRepoMock.Setup(s => s.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new RevokeSessionByIdCommandHandler(_uowMock.Object);

        // Act
        var result = await handler.Handle(new RevokeSessionByIdCommand(sessionId, userId), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        session.IsRevoked.Should().BeTrue();
        session.RevokedAt.Should().NotBeNull();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUser_WhenValid_ShouldCreateUserWithInactiveStatusAndSendActivationEmail()
    {
        // Arrange
        var email = "newuser@company.com";
        _userRepoMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>()))
            .Returns("hashed_pwd");

        User? addedUser = null;
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => addedUser = u);

        var publicUrlMock = new Mock<IPublicUrlProvider>();
        publicUrlMock.Setup(p => p.IdentityBaseUrl).Returns("https://localhost:7272/identity");

        var emailSentTcs = new TaskCompletionSource<bool>();
        _emailSenderMock
            .Setup(e => e.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => emailSentTcs.TrySetResult(true))
            .Returns(Task.CompletedTask);

        var handler = new RegisterUserCommandHandler(
            _uowMock.Object,
            _passwordHasherMock.Object,
            _cacheServiceMock.Object,
            _emailSenderMock.Object,
            publicUrlMock.Object);

        // Act
        var command = new RegisterUserCommand(email, "Password123@", "New User", null, "Dev", "0123456789");
        var result = await handler.Handle(command, CancellationToken.None);
        await Task.WhenAny(emailSentTcs.Task, Task.Delay(500));

        // Assert
        result.Should().NotBeEmpty();
        addedUser.Should().NotBeNull();
        addedUser!.Status.Should().Be(UserStatus.Inactive, "Tài khoản mới tạo phải ở trạng thái Inactive chờ kích hoạt");
        addedUser.EmailConfirmed.Should().BeFalse();
        _cacheServiceMock.Verify(c => c.SetAsync(It.Is<string>(k => k.StartsWith("email_verify:")), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
        _emailSenderMock.Verify(e => e.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyEmail_WhenTokenValid_ShouldSetStatusActiveAndEmailConfirmedTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var rawToken = "sample_raw_token_123";
        var user = new User
        {
            Id = userId,
            Email = "pending@company.com",
            Status = UserStatus.Inactive,
            EmailConfirmed = false
        };

        _cacheServiceMock.Setup(c => c.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId.ToString());

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new VerifyEmailCommandHandler(_uowMock.Object, _cacheServiceMock.Object);

        // Act
        var result = await handler.Handle(new VerifyEmailCommand(rawToken), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active, "Kích hoạt email thành công phải chuyển trạng thái sang Active");
        user.EmailConfirmed.Should().BeTrue();
        _userRepoMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
