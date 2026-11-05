using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.InitiateCall;
using EnterpriseConnect.MediaService.Domain.Entities;
using EnterpriseConnect.MediaService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.MediaService.UnitTests;

public class InitiateCallCommandHandlerTests
{
    private readonly Mock<IMediaUnitOfWork> _uowMock = new();
    private readonly Mock<ICallSessionRepository> _callRepoMock = new();
    private readonly Mock<ILiveKitTokenService> _liveKitTokenServiceMock = new();
    private readonly Mock<ICallNotificationService> _notificationServiceMock = new();

    public InitiateCallCommandHandlerTests()
    {
        _uowMock.Setup(u => u.CallSessions).Returns(_callRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task InitiateCall_Direct_ShouldCreateCallSessionSuccessfully()
    {
        var conversationId = Guid.NewGuid();
        var initiatorId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        _callRepoMock.Setup(r => r.GetActiveCallByConversationAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallSession?)null);

        var handler = new InitiateCallCommandHandler(
            _uowMock.Object, _liveKitTokenServiceMock.Object, _notificationServiceMock.Object);
        var command = new InitiateCallCommand(
            conversationId, initiatorId, CallType.Voice, CallScope.Direct, new List<Guid> { recipientId });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.InitiatorId.Should().Be(initiatorId);
        result.Data.Status.Should().Be(CallStatus.Initiated);
        result.Data.Participants.Should().HaveCount(2);

        _callRepoMock.Verify(r => r.AddAsync(It.IsAny<CallSession>(), It.IsAny<CancellationToken>()), Times.Once);
        _notificationServiceMock.Verify(n => n.NotifyIncomingCallAsync(recipientId, It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task InitiateCall_Group_ShouldGenerateLiveKitRoomAndToken()
    {
        var conversationId = Guid.NewGuid();
        var initiatorId = Guid.NewGuid();
        var member1 = Guid.NewGuid();
        var member2 = Guid.NewGuid();

        _callRepoMock.Setup(r => r.GetActiveCallByConversationAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallSession?)null);

        _liveKitTokenServiceMock.Setup(l => l.GenerateJoinToken(It.IsAny<string>(), initiatorId.ToString(), "Initiator", true, true))
            .Returns("fake_livekit_token");

        var handler = new InitiateCallCommandHandler(
            _uowMock.Object, _liveKitTokenServiceMock.Object, _notificationServiceMock.Object);
        var command = new InitiateCallCommand(
            conversationId, initiatorId, CallType.Video, CallScope.Group, new List<Guid> { member1, member2 });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.Scope.Should().Be(CallScope.Group);
        result.Data.LiveKitRoomName.Should().StartWith("room_");
        result.Data.LiveKitToken.Should().Be("fake_livekit_token");
        result.Data.Participants.Should().HaveCount(3);
    }
}
