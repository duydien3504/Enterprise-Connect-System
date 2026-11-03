using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Application.Features.Conversations.Commands;
using EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;
using EnterpriseConnect.MessagingService.Domain.Entities;
using EnterpriseConnect.MessagingService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.IdentityService.UnitTests.Application.Messaging;

public class MessagingCommandHandlersTests
{
    private readonly Mock<IMessagingUnitOfWork> _uowMock;
    private readonly Mock<IConversationRepository> _convRepoMock;
    private readonly Mock<IMessageRepository> _msgRepoMock;
    private readonly Mock<IToxicDetectionService> _toxicServiceMock;
    private readonly Mock<IKnowledgeAssistant> _knowledgeAssistantMock;

    public MessagingCommandHandlersTests()
    {
        _uowMock = new Mock<IMessagingUnitOfWork>();
        _convRepoMock = new Mock<IConversationRepository>();
        _msgRepoMock = new Mock<IMessageRepository>();
        _toxicServiceMock = new Mock<IToxicDetectionService>();
        _knowledgeAssistantMock = new Mock<IKnowledgeAssistant>();

        _uowMock.Setup(u => u.Conversations).Returns(_convRepoMock.Object);
        _uowMock.Setup(u => u.Messages).Returns(_msgRepoMock.Object);
    }

    [Fact]
    public async Task SendMessage_WhenNormalContent_ShouldSaveMessageAndReturnSuccess()
    {
        // Arrange
        var convId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var conv = new Conversation { Id = convId };
        conv.Members.Add(new ConversationMember { UserId = senderId, MemberRole = MemberRole.Member });

        _convRepoMock.Setup(r => r.GetByIdWithMembersAsync(convId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conv);

        _toxicServiceMock.Setup(t => t.AnalyzeContentAsync("Xin chào các bạn", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ToxicCheckResult(false, "None", "Hợp lệ"));

        Message? savedMsg = null;
        _msgRepoMock.Setup(m => m.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((m, _) => savedMsg = m);

        var handler = new SendMessageCommandHandler(_uowMock.Object, _toxicServiceMock.Object, _knowledgeAssistantMock.Object);

        // Act
        var result = await handler.Handle(new SendMessageCommand(convId, senderId, "Xin chào các bạn"), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.IsToxicBlocked.Should().BeFalse();
        result.Message.Should().NotBeNull();
        result.Message!.Content.Should().Be("Xin chào các bạn");
        savedMsg.Should().NotBeNull();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessage_WhenToxicContent_ShouldBlockMessageAndReturnReason()
    {
        // Arrange
        var convId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var conv = new Conversation { Id = convId };
        conv.Members.Add(new ConversationMember { UserId = senderId, MemberRole = MemberRole.Member });

        _convRepoMock.Setup(r => r.GetByIdWithMembersAsync(convId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conv);

        _toxicServiceMock.Setup(t => t.AnalyzeContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ToxicCheckResult(true, "Medium", "Phát hiện ngôn ngữ xúc phạm thô tục"));

        var handler = new SendMessageCommandHandler(_uowMock.Object, _toxicServiceMock.Object, _knowledgeAssistantMock.Object);

        // Act
        var result = await handler.Handle(new SendMessageCommand(convId, senderId, "noi_dung_chui_tuc"), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.IsToxicBlocked.Should().BeTrue();
        result.ToxicReason.Should().Contain("xúc phạm");
        result.Message.Should().BeNull();
        _msgRepoMock.Verify(m => m.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendMessage_WhenCommandSlashA_ShouldCallKnowledgeAssistantAndSaveBotResponse()
    {
        // Arrange
        var convId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var conv = new Conversation { Id = convId };
        conv.Members.Add(new ConversationMember { UserId = senderId, MemberRole = MemberRole.Member });

        _convRepoMock.Setup(r => r.GetByIdWithMembersAsync(convId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conv);

        _toxicServiceMock.Setup(t => t.AnalyzeContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ToxicCheckResult(false, "None", "Hợp lệ"));

        _knowledgeAssistantMock.Setup(k => k.AnswerQuestionAsync("Lịch nghỉ tết", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KnowledgeAnswerResult("Nghỉ từ 28 Tết đến mùng 6 Tết.", new[] { "Quy định nghỉ lễ 2026.pdf" }));

        Message? botSavedMsg = null;
        _msgRepoMock.Setup(m => m.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((m, _) => botSavedMsg = m);

        var handler = new SendMessageCommandHandler(_uowMock.Object, _toxicServiceMock.Object, _knowledgeAssistantMock.Object);

        // Act
        var result = await handler.Handle(new SendMessageCommand(convId, senderId, "/a Lịch nghỉ tết"), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.IsBotResponse.Should().BeTrue();
        result.BotAnswer.Should().Contain("Nghỉ từ 28 Tết");
        botSavedMsg.Should().NotBeNull();
        botSavedMsg!.SenderId.Should().Be(Guid.Empty); // AI Bot
        botSavedMsg.Content.Should().Contain("Quy định nghỉ lễ 2026.pdf");
    }

    [Fact]
    public async Task CreateGroupConversation_ShouldAddMembersWithOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var memberId1 = Guid.NewGuid();
        var memberId2 = Guid.NewGuid();

        Conversation? savedConv = null;
        _convRepoMock.Setup(r => r.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .Callback<Conversation, CancellationToken>((c, _) => savedConv = c);

        var handler = new CreateGroupConversationCommandHandler(_uowMock.Object);

        // Act
        var convId = await handler.Handle(new CreateGroupConversationCommand(ownerId, "Backend Team", new List<Guid> { memberId1, memberId2 }), CancellationToken.None);

        // Assert
        savedConv.Should().NotBeNull();
        savedConv!.Type.Should().Be(ConversationType.Group);
        savedConv.Name.Should().Be("Backend Team");
        savedConv.Members.Should().HaveCount(3);
        savedConv.Members.Should().Contain(m => m.UserId == ownerId && m.MemberRole == MemberRole.Owner);
        savedConv.Members.Should().Contain(m => m.UserId == memberId1 && m.MemberRole == MemberRole.Member);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
