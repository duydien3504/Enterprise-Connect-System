using EnterpriseConnect.IdentityService.Domain.Enums;
using EnterpriseConnect.IdentityService.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace EnterpriseConnect.IdentityService.UnitTests.Infrastructure;

public class MemoryPresenceServiceTests
{
    [Fact]
    public async Task UpdatePresence_ShouldStoreStatus()
    {
        // Arrange
        var presenceService = new MemoryPresenceService();
        var userId = Guid.NewGuid();

        // Act
        await presenceService.SetPresenceAsync(userId, PresenceStatus.Online, DeviceType.Web);
        var status = await presenceService.GetPresenceAsync(userId);

        // Assert
        status.Should().Be(PresenceStatus.Online);
    }

    [Fact]
    public async Task GetUserPresence_WhenUserNotTracked_ShouldReturnOffline()
    {
        // Arrange
        var presenceService = new MemoryPresenceService();
        var unknownUserId = Guid.NewGuid();

        // Act
        var status = await presenceService.GetPresenceAsync(unknownUserId);

        // Assert
        status.Should().Be(PresenceStatus.Offline);
    }

    [Fact]
    public async Task GetBatchPresence_ShouldReturnDictionaryForRequestedUsers()
    {
        // Arrange
        var presenceService = new MemoryPresenceService();
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var user3 = Guid.NewGuid();

        await presenceService.SetPresenceAsync(user1, PresenceStatus.Online, DeviceType.Desktop);
        await presenceService.SetPresenceAsync(user2, PresenceStatus.Busy, DeviceType.Mobile);

        // Act
        var batch = await presenceService.GetBatchPresenceAsync(new[] { user1, user2, user3 });

        // Assert
        batch.Should().HaveCount(3);
        batch[user1].Should().Be(PresenceStatus.Online);
        batch[user2].Should().Be(PresenceStatus.Busy);
        batch[user3].Should().Be(PresenceStatus.Offline);
    }
}
