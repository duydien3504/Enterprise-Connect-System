using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Application.Features.Conversations.Queries;
using EnterpriseConnect.MessagingService.Domain.Entities;
using EnterpriseConnect.MessagingService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.IdentityService.UnitTests.Application.Messaging;

public class GetConversationMembersQueryHandlerTests
{
    private readonly Mock<IMessagingUnitOfWork> _uowMock;
    private readonly Mock<IConversationRepository> _convRepoMock;

    public GetConversationMembersQueryHandlerTests()
    {
        _uowMock = new Mock<IMessagingUnitOfWork>();
        _convRepoMock = new Mock<IConversationRepository>();
        _uowMock.Setup(u => u.Conversations).Returns(_convRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIsMember_ShouldReturnMembersOrderedByRoleAndJoinedAt()
    {
        // Arrange
        var convId = Guid.NewGuid();
        var callerUserId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();

        var conv = new Conversation { Id = convId };
        conv.Members.Add(new ConversationMember
        {
            UserId = callerUserId,
            MemberRole = MemberRole.Member,
            Nickname = "Member User",
            JoinedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
        });
        conv.Members.Add(new ConversationMember
        {
            UserId = adminUserId,
            MemberRole = MemberRole.Admin,
            Nickname = "Admin User",
            JoinedAt = DateTimeOffset.UtcNow.AddMinutes(-30)
        });
        conv.Members.Add(new ConversationMember
        {
            UserId = ownerUserId,
            MemberRole = MemberRole.Owner,
            Nickname = "Owner User",
            JoinedAt = DateTimeOffset.UtcNow.AddMinutes(-60)
        });

        _convRepoMock.Setup(r => r.GetByIdWithMembersAsync(convId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conv);

        var handler = new GetConversationMembersQueryHandler(_uowMock.Object);

        // Act
        var result = await handler.Handle(new GetConversationMembersQuery(convId, callerUserId), CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result[0].UserId.Should().Be(ownerUserId);
        result[0].MemberRole.Should().Be(MemberRole.Owner);
        result[1].UserId.Should().Be(adminUserId);
        result[1].MemberRole.Should().Be(MemberRole.Admin);
        result[2].UserId.Should().Be(callerUserId);
        result[2].MemberRole.Should().Be(MemberRole.Member);
        result[2].Nickname.Should().Be("Member User");
    }

    [Fact]
    public async Task Handle_WhenUserIsNotMember_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var convId = Guid.NewGuid();
        var callerUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var conv = new Conversation { Id = convId };
        conv.Members.Add(new ConversationMember { UserId = otherUserId, MemberRole = MemberRole.Owner });

        _convRepoMock.Setup(r => r.GetByIdWithMembersAsync(convId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conv);

        var handler = new GetConversationMembersQueryHandler(_uowMock.Object);

        // Act & Assert
        var act = () => handler.Handle(new GetConversationMembersQuery(convId, callerUserId), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*không phải thành viên*");
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var convId = Guid.NewGuid();
        var callerUserId = Guid.NewGuid();

        _convRepoMock.Setup(r => r.GetByIdWithMembersAsync(convId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        var handler = new GetConversationMembersQueryHandler(_uowMock.Object);

        // Act & Assert
        var act = () => handler.Handle(new GetConversationMembersQuery(convId, callerUserId), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Không tìm thấy cuộc trò chuyện*");
    }
}
