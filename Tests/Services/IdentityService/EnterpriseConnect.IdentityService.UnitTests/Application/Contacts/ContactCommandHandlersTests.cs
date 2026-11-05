using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Contacts.Commands;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.IdentityService.UnitTests.Application.Contacts;

public class ContactCommandHandlersTests
{
    private readonly Mock<IIdentityUnitOfWork> _uowMock;
    private readonly Mock<IContactRepository> _contactRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;

    public ContactCommandHandlersTests()
    {
        _uowMock = new Mock<IIdentityUnitOfWork>();
        _contactRepoMock = new Mock<IContactRepository>();
        _userRepoMock = new Mock<IUserRepository>();

        _uowMock.Setup(u => u.Contacts).Returns(_contactRepoMock.Object);
        _uowMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
    }

    [Fact]
    public async Task SendContactRequest_WhenUsersAreNotConnected_ShouldCreatePendingContact()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var handler = new SendContactRequestCommandHandler(_uowMock.Object);

        _userRepoMock.Setup(r => r.GetByIdAsync(receiverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = receiverId, Email = "test@company.com", Status = UserStatus.Active });

        _contactRepoMock.Setup(r => r.GetContactBetweenUsersAsync(requesterId, receiverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        Contact? savedContact = null;
        _contactRepoMock.Setup(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .Callback<Contact, CancellationToken>((c, _) => savedContact = c);

        // Act
        var result = await handler.Handle(new SendContactRequestCommand(requesterId, receiverId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be((int)ContactStatus.Pending);
        savedContact.Should().NotBeNull();
        savedContact!.RequesterId.Should().Be(requesterId);
        savedContact.ReceiverId.Should().Be(receiverId);
        savedContact.Status.Should().Be(ContactStatus.Pending);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendContactRequest_WhenCrossRequestPending_ShouldAutoAccept()
    {
        // Arrange (Receiver already sent a request to Requester)
        var requesterId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var handler = new SendContactRequestCommandHandler(_uowMock.Object);

        _userRepoMock.Setup(r => r.GetByIdAsync(receiverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = receiverId, Email = "test@company.com", Status = UserStatus.Active });

        var existingPendingFromReceiver = new Contact
        {
            Id = Guid.NewGuid(),
            RequesterId = receiverId,
            ReceiverId = requesterId,
            Status = ContactStatus.Pending
        };

        _contactRepoMock.Setup(r => r.GetContactBetweenUsersAsync(requesterId, receiverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPendingFromReceiver);

        // Act
        var result = await handler.Handle(new SendContactRequestCommand(requesterId, receiverId), CancellationToken.None);

        // Assert (Auto Accept)
        result.Should().NotBeNull();
        result.Status.Should().Be((int)ContactStatus.Accepted);
        existingPendingFromReceiver.Status.Should().Be(ContactStatus.Accepted);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RespondContactRequest_WhenAccepted_ShouldSetStatusAccepted()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var handler = new RespondContactRequestCommandHandler(_uowMock.Object);

        var existing = new Contact
        {
            Id = contactId,
            RequesterId = requesterId,
            ReceiverId = receiverId,
            Status = ContactStatus.Pending
        };

        _contactRepoMock.Setup(r => r.GetByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await handler.Handle(new RespondContactRequestCommand(contactId, receiverId, true), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        existing.Status.Should().Be(ContactStatus.Accepted);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BlockUser_WhenNew_ShouldCreateBlockedRecord()
    {
        // Arrange
        var blockerId = Guid.NewGuid();
        var blockedUserId = Guid.NewGuid();
        var handler = new BlockUserCommandHandler(_uowMock.Object);

        _contactRepoMock.Setup(r => r.GetContactBetweenUsersAsync(blockerId, blockedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        Contact? savedBlock = null;
        _contactRepoMock.Setup(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .Callback<Contact, CancellationToken>((c, _) => savedBlock = c);

        // Act
        var result = await handler.Handle(new BlockUserCommand(blockerId, blockedUserId), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        savedBlock.Should().NotBeNull();
        savedBlock!.RequesterId.Should().Be(blockerId);
        savedBlock.ReceiverId.Should().Be(blockedUserId);
        savedBlock.Status.Should().Be(ContactStatus.Blocked);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnblockUser_WhenBlockedByRequester_ShouldHardDeleteAndReturnTrue()
    {
        // Arrange
        var blockerId = Guid.NewGuid();
        var blockedUserId = Guid.NewGuid();
        var handler = new UnblockUserCommandHandler(_uowMock.Object);

        var existingBlocked = new Contact
        {
            Id = Guid.NewGuid(),
            RequesterId = blockerId,
            ReceiverId = blockedUserId,
            Status = ContactStatus.Blocked
        };

        _contactRepoMock.Setup(r => r.GetContactBetweenUsersAsync(blockerId, blockedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBlocked);

        _contactRepoMock.Setup(r => r.HardDeleteBetweenUsersAsync(blockerId, blockedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(new UnblockUserCommand(blockerId, blockedUserId), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _contactRepoMock.Verify(r => r.HardDeleteBetweenUsersAsync(blockerId, blockedUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnblockUser_WhenNotBlockedOrNotBlocker_ShouldReturnFalse()
    {
        // Arrange (Attempting to unblock someone you didn't block)
        var blockerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var handler = new UnblockUserCommandHandler(_uowMock.Object);

        _contactRepoMock.Setup(r => r.GetContactBetweenUsersAsync(otherUserId, blockerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        // Act
        var result = await handler.Handle(new UnblockUserCommand(otherUserId, blockerId), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _contactRepoMock.Verify(r => r.HardDeleteBetweenUsersAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveContact_WhenAccepted_ShouldHardDeleteAndReturnTrue()
    {
        // Arrange
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var handler = new RemoveContactCommandHandler(_uowMock.Object);

        var existingFriendship = new Contact
        {
            Id = Guid.NewGuid(),
            RequesterId = userA,
            ReceiverId = userB,
            Status = ContactStatus.Accepted
        };

        _contactRepoMock.Setup(r => r.GetContactBetweenUsersAsync(userA, userB, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFriendship);

        _contactRepoMock.Setup(r => r.HardDeleteBetweenUsersAsync(userA, userB, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(new RemoveContactCommand(userA, userB), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _contactRepoMock.Verify(r => r.HardDeleteBetweenUsersAsync(userA, userB, It.IsAny<CancellationToken>()), Times.Once);
    }
}
