using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.EndCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.InitiateCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.RespondCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.UpdateMediaState;
using EnterpriseConnect.MediaService.Domain.Entities;
using EnterpriseConnect.MediaService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.MediaService.UnitTests;

public class CallCommandHandlersTests
{
    private readonly Mock<IMediaUnitOfWork> _uowMock;
    private readonly Mock<ICallSessionRepository> _callRepoMock;
    private readonly Mock<ILiveKitTokenService> _liveKitTokenServiceMock;
    private readonly Mock<ICallNotificationService> _notificationServiceMock;

    public CallCommandHandlersTests()
    {
        _uowMock = new Mock<IMediaUnitOfWork>();
        _callRepoMock = new Mock<ICallSessionRepository>();
        _liveKitTokenServiceMock = new Mock<ILiveKitTokenService>();
        _notificationServiceMock = new Mock<ICallNotificationService>();

        _uowMock.Setup(u => u.CallSessions).Returns(_callRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task InitiateCall_Direct_ShouldCreateCallSessionSuccessfully()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var initiatorId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        _callRepoMock.Setup(r => r.GetActiveCallByConversationAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallSession?)null);

        var handler = new InitiateCallCommandHandler(_uowMock.Object, _liveKitTokenServiceMock.Object, _notificationServiceMock.Object);
        var command = new InitiateCallCommand(conversationId, initiatorId, CallType.Voice, CallScope.Direct, new List<Guid> { recipientId });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
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
        // Arrange
        var conversationId = Guid.NewGuid();
        var initiatorId = Guid.NewGuid();
        var member1 = Guid.NewGuid();
        var member2 = Guid.NewGuid();

        _callRepoMock.Setup(r => r.GetActiveCallByConversationAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallSession?)null);

        _liveKitTokenServiceMock.Setup(l => l.GenerateJoinToken(It.IsAny<string>(), initiatorId.ToString(), "Initiator", true, true))
            .Returns("fake_livekit_token");

        var handler = new InitiateCallCommandHandler(_uowMock.Object, _liveKitTokenServiceMock.Object, _notificationServiceMock.Object);
        var command = new InitiateCallCommand(conversationId, initiatorId, CallType.Video, CallScope.Group, new List<Guid> { member1, member2 });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data!.Scope.Should().Be(CallScope.Group);
        result.Data.LiveKitRoomName.Should().StartWith("room_");
        result.Data.LiveKitToken.Should().Be("fake_livekit_token");
        result.Data.Participants.Should().HaveCount(3);
    }

    [Fact]
    public async Task RespondCall_Accept_ShouldTransitionToOngoing()
    {
        // Arrange
        var callId = Guid.NewGuid();
        var initiatorId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        var callSession = new CallSession
        {
            Id = callId,
            InitiatorId = initiatorId,
            Scope = CallScope.Direct,
            Status = CallStatus.Initiated,
            Participants = new List<CallParticipant>
            {
                new() { UserId = initiatorId, Status = ParticipantStatus.Joined },
                new() { UserId = recipientId, Status = ParticipantStatus.Inviting }
            }
        };

        _callRepoMock.Setup(r => r.GetByIdWithParticipantsAsync(callId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(callSession);

        var handler = new RespondCallCommandHandler(_uowMock.Object, _liveKitTokenServiceMock.Object, _notificationServiceMock.Object);
        var command = new RespondCallCommand(callId, recipientId, true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data!.Status.Should().Be(CallStatus.Ongoing);
        result.Data.ConnectedAt.Should().NotBeNull();

        var participant = callSession.Participants.First(p => p.UserId == recipientId);
        participant.Status.Should().Be(ParticipantStatus.Joined);

        _notificationServiceMock.Verify(n => n.NotifyCallAnsweredAsync(callId, recipientId, It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task EndCall_DirectCall_ShouldEndSessionAndCalculateDuration()
    {
        // Arrange
        var callId = Guid.NewGuid();
        var initiatorId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        var connectedTime = DateTimeOffset.UtcNow.AddMinutes(-5);

        var callSession = new CallSession
        {
            Id = callId,
            InitiatorId = initiatorId,
            Scope = CallScope.Direct,
            Status = CallStatus.Ongoing,
            ConnectedAt = connectedTime,
            Participants = new List<CallParticipant>
            {
                new() { UserId = initiatorId, Status = ParticipantStatus.Joined },
                new() { UserId = recipientId, Status = ParticipantStatus.Joined }
            }
        };

        _callRepoMock.Setup(r => r.GetByIdWithParticipantsAsync(callId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(callSession);

        var handler = new EndCallCommandHandler(_uowMock.Object, _notificationServiceMock.Object);
        var command = new EndCallCommand(callId, recipientId, "Call completed normally");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data!.Status.Should().Be(CallStatus.Ended);
        result.Data.DurationSeconds.Should().BeGreaterThan(290);
        result.Data.EndReason.Should().Be("Call completed normally");

        _notificationServiceMock.Verify(n => n.NotifyCallEndedAsync(callId, "Call completed normally", It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMediaState_ShouldUpdateParticipantFlags()
    {
        // Arrange
        var callId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var callSession = new CallSession
        {
            Id = callId,
            Status = CallStatus.Ongoing,
            Participants = new List<CallParticipant>
            {
                new() { UserId = userId, IsMuted = false, IsCameraOn = true, IsScreenSharing = false }
            }
        };

        _callRepoMock.Setup(r => r.GetByIdWithParticipantsAsync(callId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(callSession);

        var handler = new UpdateMediaStateCommandHandler(_uowMock.Object, _notificationServiceMock.Object);
        var command = new UpdateMediaStateCommand(callId, userId, IsMuted: true, IsCameraOn: false, IsScreenSharing: true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data!.IsMuted.Should().BeTrue();
        result.Data.IsCameraOn.Should().BeFalse();
        result.Data.IsScreenSharing.Should().BeTrue();

        _notificationServiceMock.Verify(n => n.NotifyMediaStateChangedAsync(callId, userId, true, false, true), Times.Once);
    }
}
