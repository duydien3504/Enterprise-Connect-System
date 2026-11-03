using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EnterpriseConnect.IdentityService.UnitTests.Domain;

public class ContactTests
{
    [Fact]
    public void CreateContact_ShouldInitialize_WithPendingStatus()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();

        // Act
        var contact = new Contact
        {
            RequesterId = requesterId,
            ReceiverId = receiverId,
            Status = ContactStatus.Pending
        };

        // Assert
        contact.RequesterId.Should().Be(requesterId);
        contact.ReceiverId.Should().Be(receiverId);
        contact.Status.Should().Be(ContactStatus.Pending);
    }

    [Fact]
    public void AcceptContact_ShouldUpdateStatusToAccepted()
    {
        // Arrange
        var contact = new Contact
        {
            RequesterId = Guid.NewGuid(),
            ReceiverId = Guid.NewGuid(),
            Status = ContactStatus.Pending
        };

        // Act
        contact.Status = ContactStatus.Accepted;

        // Assert
        contact.Status.Should().Be(ContactStatus.Accepted);
    }

    [Fact]
    public void BlockContact_ShouldUpdateStatusToBlocked()
    {
        // Arrange
        var contact = new Contact
        {
            RequesterId = Guid.NewGuid(),
            ReceiverId = Guid.NewGuid(),
            Status = ContactStatus.Accepted
        };

        // Act
        contact.Status = ContactStatus.Blocked;

        // Assert
        contact.Status.Should().Be(ContactStatus.Blocked);
    }
}
