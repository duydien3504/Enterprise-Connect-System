using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.RespondCall;
using EnterpriseConnect.MediaService.Domain.Entities;
using EnterpriseConnect.MediaService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.MediaService.UnitTests;

public class RespondCallCommandHandlerTests
{
    private readonly Mock<IMediaUnitOfWork> _uowMock = new();
    private readonly Mock<ICallSessionRepository> _callRepoMock = new();
    private readonly Mock<ILiveKitTokenService> _liveKitTokenServiceMock = new();
    private readonly Mock<ICallNotificationService> _notificationServiceMock = new();

    public RespondCallCommandHandlerTests()
    {
        _uowMock.Setup(u => u.CallSessions).Returns(_callRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task RespondCall_Accept_ShouldTransitionToOngoing()
    {
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

        var handler = new RespondCallCommandHandler(
            _uowMock.Object, _liveKitTokenServiceMock.Object, _notificationServiceMock.Object);
        var command = new RespondCallCommand(callId, recipientId, true);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.Status.Should().Be(CallStatus.Ongoing);
        result.Data.ConnectedAt.Should().NotBeNull();

        var participant = callSession.Participants.First(p => p.UserId == recipientId);
        participant.Status.Should().Be(ParticipantStatus.Joined);

        _notificationServiceMock.Verify(n => n.NotifyCallAnsweredAsync(callId, recipientId, It.IsAny<object>()), Times.Once);
    }
}
