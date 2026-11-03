using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Users.Commands;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.IdentityService.UnitTests.Application.Users;

public class UserCommandHandlersTests
{
    private readonly Mock<IIdentityUnitOfWork> _uowMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUserSessionRepository> _sessionRepoMock;
    private readonly Mock<IImageValidator> _imageValidatorMock;
    private readonly Mock<ICloudinaryStorageService> _cloudinaryMock;

    public UserCommandHandlersTests()
    {
        _uowMock = new Mock<IIdentityUnitOfWork>();
        _userRepoMock = new Mock<IUserRepository>();
        _sessionRepoMock = new Mock<IUserSessionRepository>();
        _imageValidatorMock = new Mock<IImageValidator>();
        _cloudinaryMock = new Mock<ICloudinaryStorageService>();

        _uowMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
        _uowMock.Setup(u => u.UserSessions).Returns(_sessionRepoMock.Object);
    }

    [Fact]
    public async Task LockUser_ShouldSetStatusInactive_AndRevokeAllSessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Status = UserStatus.Active };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new LockUserCommandHandler(_uowMock.Object);

        // Act
        var result = await handler.Handle(new LockUserCommand(userId, "Vi phạm quy định an toàn"), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Inactive);
        user.LockReason.Should().Be("Vi phạm quy định an toàn");
        user.LockedAt.Should().NotBeNull();
        _sessionRepoMock.Verify(s => s.RevokeAllSessionsByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnlockUser_ShouldRestoreStatusActive_AndClearLockReason()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Status = UserStatus.Inactive, LockReason = "Locked", LockedAt = DateTimeOffset.UtcNow };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UnlockUserCommandHandler(_uowMock.Object);

        // Act
        var result = await handler.Handle(new UnlockUserCommand(userId), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
        user.LockReason.Should().BeNull();
        user.LockedAt.Should().BeNull();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_ShouldUpdateFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, FullName = "Old Name", Phone = "0123456789" };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new UpdateProfileCommandHandler(_uowMock.Object);

        // Act
        var result = await handler.Handle(new UpdateProfileCommand(userId, "New Name", "0987654321", "Senior Dev"), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.FullName.Should().Be("New Name");
        user.Phone.Should().Be("0987654321");
        user.Position.Should().Be("Senior Dev");
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadAvatar_WhenValidImage_ShouldSanitize_UploadToCloudinary_AndUpdateUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, AvatarUrl = null };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var dummyBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // fake jpeg
        _imageValidatorMock.Setup(v => v.ValidateAndSanitizeAvatarAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dummyBytes);

        _cloudinaryMock.Setup(c => c.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), "avatars", It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://res.cloudinary.com/demo/image/upload/v1/avatars/avatar.jpg");

        var handler = new UploadAvatarCommandHandler(_uowMock.Object, _imageValidatorMock.Object, _cloudinaryMock.Object);

        using var inputStream = new MemoryStream(dummyBytes);

        // Act
        var result = await handler.Handle(new UploadAvatarCommand(userId, inputStream, "photo.jpg"), CancellationToken.None);

        // Assert
        result.Should().Be("https://res.cloudinary.com/demo/image/upload/v1/avatars/avatar.jpg");
        user.AvatarUrl.Should().Be(result);
        _imageValidatorMock.Verify(v => v.ValidateAndSanitizeAvatarAsync(It.IsAny<Stream>(), "photo.jpg", It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
