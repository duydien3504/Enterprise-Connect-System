using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Application.Features.Conversations.Commands;
using EnterpriseConnect.MessagingService.Application.Features.Conversations.Queries;
using EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;
using EnterpriseConnect.MessagingService.Application.Features.Messages.Queries;
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
    private readonly Mock<IAiSummaryRepository> _aiSummaryRepoMock;
    private readonly Mock<IToxicDetectionService> _toxicServiceMock;
    private readonly Mock<IKnowledgeAssistant> _knowledgeAssistantMock;

    public MessagingCommandHandlersTests()
    {
        _uowMock = new Mock<IMessagingUnitOfWork>();
        _convRepoMock = new Mock<IConversationRepository>();
        _msgRepoMock = new Mock<IMessageRepository>();
        _aiSummaryRepoMock = new Mock<IAiSummaryRepository>();
        _toxicServiceMock = new Mock<IToxicDetectionService>();
        _knowledgeAssistantMock = new Mock<IKnowledgeAssistant>();

        _uowMock.Setup(u => u.Conversations).Returns(_convRepoMock.Object);
        _uowMock.Setup(u => u.Messages).Returns(_msgRepoMock.Object);
        _uowMock.Setup(u => u.AiSummaries).Returns(_aiSummaryRepoMock.Object);
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
    public async Task CreateDirectConversation_WhenNew_ShouldCreateConversation()
    {
        // Arrange
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        _convRepoMock.Setup(r => r.FindDirectConversationAsync(userA, userB, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        Conversation? savedConv = null;
        _convRepoMock.Setup(r => r.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .Callback<Conversation, CancellationToken>((c, _) => savedConv = c);

        var handler = new CreateDirectConversationCommandHandler(_uowMock.Object);

        // Act
        var convId = await handler.Handle(new CreateDirectConversationCommand(userA, userB), CancellationToken.None);

        // Assert
        savedConv.Should().NotBeNull();
        savedConv!.Type.Should().Be(ConversationType.Direct);
        savedConv.Members.Should().HaveCount(2);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

    [Fact]
    public async Task SummarizeConversation_ShouldCallAiAndPersistSummary()
    {
        // Arrange
        var convId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var conv = new Conversation { Id = convId };

        _convRepoMock.Setup(r => r.GetByIdWithMembersAsync(convId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conv);

        _msgRepoMock.Setup(m => m.GetConversationMessagesAsync(convId, 0, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Message>
            {
                new() { Id = Guid.NewGuid(), ConversationId = convId, Content = "Bàn về dự án", CreatedAt = DateTimeOffset.UtcNow }
            });

        var summarizerMock = new Mock<IConversationSummarizer>();
        summarizerMock.Setup(s => s.SummarizeAsync(It.IsAny<IEnumerable<(string, string, DateTimeOffset)>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConversationSummaryResult("Tóm tắt hội thoại", new[] { "Điểm 1" }, new[] { "Quyết định 1" }, new ActionItemDto[] { new("Task 1", "Duy") }, new[] { "User A" }));

        AiSummary? savedSummary = null;
        _aiSummaryRepoMock.Setup(a => a.AddAsync(It.IsAny<AiSummary>(), It.IsAny<CancellationToken>()))
            .Callback<AiSummary, CancellationToken>((s, _) => savedSummary = s);

        var handler = new SummarizeConversationCommandHandler(_uowMock.Object, summarizerMock.Object);

        // Act
        var result = await handler.Handle(new SummarizeConversationCommand(convId, userId, 50), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Summary.Should().Be("Tóm tắt hội thoại");
        savedSummary.Should().NotBeNull();
        savedSummary!.SummaryText.Should().Be("Tóm tắt hội thoại");
        savedSummary.ConversationId.Should().Be(convId);
        _aiSummaryRepoMock.Verify(a => a.AddAsync(It.IsAny<AiSummary>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SummarizeConversation_WhenEnglishOption_ShouldPassEnglishToSummarizer()
    {
        // Arrange
        var convId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var conv = new Conversation { Id = convId };
        conv.Members.Add(new ConversationMember { UserId = userId, MemberRole = MemberRole.Member });

        _convRepoMock.Setup(r => r.GetByIdWithMembersAsync(convId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conv);

        _msgRepoMock.Setup(m => m.GetConversationMessagesAsync(convId, 0, 30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Message>
            {
                new() { Id = Guid.NewGuid(), ConversationId = convId, Content = "Discussing Q4 roadmap", CreatedAt = DateTimeOffset.UtcNow }
            });

        var summarizerMock = new Mock<IConversationSummarizer>();
        string? passedLanguage = null;
        summarizerMock.Setup(s => s.SummarizeAsync(It.IsAny<IEnumerable<(string, string, DateTimeOffset)>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<(string, string, DateTimeOffset)>, string, CancellationToken>((_, lang, _) => passedLanguage = lang)
            .ReturnsAsync(new ConversationSummaryResult("English summary", new[] { "Point 1" }, new[] { "Decision 1" }, new ActionItemDto[] { new("Task 1", "John") }, new[] { "User A" }));

        _aiSummaryRepoMock.Setup(a => a.AddAsync(It.IsAny<AiSummary>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new SummarizeConversationCommandHandler(_uowMock.Object, summarizerMock.Object);

        // Act
        var result = await handler.Handle(new SummarizeConversationCommand(convId, userId, 30, "en"), CancellationToken.None);

        // Assert
        result.Summary.Should().Be("English summary");
        passedLanguage.Should().Be("en");
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TogglePinMessage_ShouldInvertPinStatus()
    {
        // Arrange
        var msgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var message = new Message { Id = msgId, IsPinned = false };

        _msgRepoMock.Setup(m => m.GetByIdAsync(msgId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        var handler = new TogglePinMessageCommandHandler(_uowMock.Object);

        // Act
        var isPinned = await handler.Handle(new TogglePinMessageCommand(msgId, userId), CancellationToken.None);

        // Assert
        isPinned.Should().BeTrue();
        message.IsPinned.Should().BeTrue();
        message.PinnedByUserId.Should().Be(userId);
        _msgRepoMock.Verify(m => m.UpdateAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserConversations_ShouldReturnMappedDtoList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conv1 = new Conversation { Id = Guid.NewGuid(), Type = ConversationType.Direct, Name = "1-1" };
        var conv2 = new Conversation { Id = Guid.NewGuid(), Type = ConversationType.Group, Name = "Nhóm Core" };

        _convRepoMock.Setup(r => r.GetUserConversationsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Conversation> { conv1, conv2 });

        var handler = new GetUserConversationsQueryHandler(_uowMock.Object);

        // Act
        var result = await handler.Handle(new GetUserConversationsQuery(userId), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("1-1");
        result[1].Name.Should().Be("Nhóm Core");
    }

    [Fact]
    public async Task GetConversationMessages_WhenUserIsMember_ShouldReturnMessages()
    {
        // Arrange
        var convId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var conv = new Conversation { Id = convId };
        conv.Members.Add(new ConversationMember { UserId = userId, MemberRole = MemberRole.Member });

        _convRepoMock.Setup(r => r.GetByIdWithMembersAsync(convId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conv);

        _msgRepoMock.Setup(m => m.GetConversationMessagesAsync(convId, 0, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Message>
            {
                new() { Id = Guid.NewGuid(), ConversationId = convId, Content = "Tin nhắn test 1", SenderId = userId }
            });

        var handler = new GetConversationMessagesQueryHandler(_uowMock.Object);

        // Act
        var result = await handler.Handle(new GetConversationMessagesQuery(convId, userId, 0, 50), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Content.Should().Be("Tin nhắn test 1");
    }
}
