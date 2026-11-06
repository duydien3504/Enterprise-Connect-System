using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;
using EnterpriseConnect.MessagingService.Domain.Entities;
using EnterpriseConnect.MessagingService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.MessagingService.UnitTests;

public class SendMessageCommandHandlerTests
{
    private readonly Mock<IMessagingUnitOfWork> _uowMock = new();
    private readonly Mock<IConversationRepository> _convRepoMock = new();
    private readonly Mock<IMessageRepository> _msgRepoMock = new();
    private readonly Mock<IToxicDetectionService> _toxicMock = new();
    private readonly Mock<IKnowledgeAssistant> _assistantMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    public SendMessageCommandHandlerTests()
    {
        _uowMock.Setup(u => u.Conversations).Returns(_convRepoMock.Object);
        _uowMock.Setup(u => u.Messages).Returns(_msgRepoMock.Object);
        _toxicMock.Setup(t => t.AnalyzeContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ToxicCheckResult(false, "Low", string.Empty));
    }

    [Fact]
    public async Task Handle_AiBotMessage_ShouldPropagateDepartmentAndRoleFromCurrentUser()
    {
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var conv = new Conversation { Id = conversationId, Type = ConversationType.Direct };
        conv.Members.Add(new ConversationMember { ConversationId = conversationId, UserId = userId });
        _convRepoMock.Setup(c => c.GetByIdWithMembersAsync(conversationId, It.IsAny<CancellationToken>())).ReturnsAsync(conv);

        _currentUserMock.Setup(u => u.DepartmentId).Returns(deptId);
        _currentUserMock.Setup(u => u.RoleIds).Returns(new List<Guid> { roleId });

        _assistantMock.Setup(a => a.AnswerQuestionAsync("Quy định nghỉ phép?", deptId, It.Is<IEnumerable<Guid>>(r => r.Contains(roleId)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KnowledgeAnswerResult("Quy chế nghỉ phép là 12 ngày/năm.", new[] { "HR Policy" }));

        var handler = new SendMessageCommandHandler(_uowMock.Object, _toxicMock.Object, _assistantMock.Object, _currentUserMock.Object);
        var command = new SendMessageCommand(conversationId, userId, "/a Quy định nghỉ phép?", MessageType.Text);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.IsBotResponse.Should().BeTrue();
        result.BotAnswer.Should().Be("Quy chế nghỉ phép là 12 ngày/năm.");
        _assistantMock.Verify(a => a.AnswerQuestionAsync("Quy định nghỉ phép?", deptId, It.Is<IEnumerable<Guid>>(r => r.Contains(roleId)), It.IsAny<CancellationToken>()), Times.Once);
        _msgRepoMock.Verify(m => m.AddAsync(It.Is<Message>(msg => msg.SenderId == Guid.Empty), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NormalMessage_ShouldNotInvokeKnowledgeAssistant()
    {
        var conversationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var conv = new Conversation { Id = conversationId, Type = ConversationType.Direct };
        conv.Members.Add(new ConversationMember { ConversationId = conversationId, UserId = userId });
        _convRepoMock.Setup(c => c.GetByIdWithMembersAsync(conversationId, It.IsAny<CancellationToken>())).ReturnsAsync(conv);

        var handler = new SendMessageCommandHandler(_uowMock.Object, _toxicMock.Object, _assistantMock.Object, _currentUserMock.Object);
        var command = new SendMessageCommand(conversationId, userId, "Xin chào team!", MessageType.Text);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.IsBotResponse.Should().BeFalse();
        _assistantMock.Verify(a => a.AnswerQuestionAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()), Times.Never);
        _msgRepoMock.Verify(m => m.AddAsync(It.Is<Message>(msg => msg.SenderId == userId), It.IsAny<CancellationToken>()), Times.Once);
    }
}
