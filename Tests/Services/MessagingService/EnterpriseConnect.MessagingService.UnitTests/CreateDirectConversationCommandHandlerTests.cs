using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Application.Features.Conversations.Commands;
using EnterpriseConnect.MessagingService.Domain.Entities;
using EnterpriseConnect.MessagingService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.MessagingService.UnitTests;

public class CreateDirectConversationCommandHandlerTests
{
    private readonly Mock<IMessagingUnitOfWork> _uowMock = new();
    private readonly Mock<IConversationRepository> _convRepoMock = new();

    public CreateDirectConversationCommandHandlerTests()
    {
        _uowMock.Setup(u => u.Conversations).Returns(_convRepoMock.Object);
    }

    [Fact]
    public async Task Handle_DirectConversationWithSelf_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        var handler = new CreateDirectConversationCommandHandler(_uowMock.Object);
        var command = new CreateDirectConversationCommand(userId, userId);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Không thể tạo cuộc trò chuyện trực tiếp với chính mình*");
    }

    [Fact]
    public async Task Handle_DirectConversationAlreadyExists_ReturnsExistingIdWithoutCreatingNew()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var existingConvId = Guid.NewGuid();

        var existingConv = new Conversation
        {
            Id = existingConvId,
            Type = ConversationType.Direct
        };
        existingConv.Members.Add(new ConversationMember { ConversationId = existingConvId, UserId = userA, MemberRole = MemberRole.Member });
        existingConv.Members.Add(new ConversationMember { ConversationId = existingConvId, UserId = userB, MemberRole = MemberRole.Member });

        _convRepoMock.Setup(r => r.FindDirectConversationAsync(userA, userB, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingConv);

        var handler = new CreateDirectConversationCommandHandler(_uowMock.Object);
        var command = new CreateDirectConversationCommand(userA, userB);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(existingConvId);
        _convRepoMock.Verify(r => r.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DirectConversationDoesNotExist_CreatesNewConversationWithBothMembers()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        Conversation? capturedConv = null;

        _convRepoMock.Setup(r => r.FindDirectConversationAsync(userA, userB, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        _convRepoMock.Setup(r => r.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .Callback<Conversation, CancellationToken>((c, _) =>
            {
                capturedConv = c;
            })
            .Returns(Task.CompletedTask);

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateDirectConversationCommandHandler(_uowMock.Object);
        var command = new CreateDirectConversationCommand(userA, userB);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        capturedConv.Should().NotBeNull();
        capturedConv!.Type.Should().Be(ConversationType.Direct);
        capturedConv.Members.Should().HaveCount(2);
        capturedConv.Members.Select(m => m.UserId).Should().Contain(new[] { userA, userB });
        capturedConv.Members.Should().OnlyContain(m => m.MemberRole == MemberRole.Member);

        _convRepoMock.Verify(r => r.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConcurrentRequests_DoubleCheckPreventsDuplicateCreation()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var firstCreatedId = Guid.NewGuid();

        var callCount = 0;
        _convRepoMock.Setup(r => r.FindDirectConversationAsync(userA, userB, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                // First call: returns null
                if (callCount <= 1) return null;
                // Second or subsequent call (e.g. after lock or first commit): returns conversation
                return new Conversation { Id = firstCreatedId, Type = ConversationType.Direct };
            });

        _convRepoMock.Setup(r => r.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateDirectConversationCommandHandler(_uowMock.Object);
        var command = new CreateDirectConversationCommand(userA, userB);

        // Run concurrently
        var task1 = handler.Handle(command, CancellationToken.None);
        var task2 = handler.Handle(command, CancellationToken.None);

        var results = await Task.WhenAll(task1, task2);

        // At most one creation should have happened
        _convRepoMock.Verify(r => r.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()), Times.AtMostOnce);
    }
}
