using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.EndCall;
using EnterpriseConnect.MediaService.Domain.Entities;
using EnterpriseConnect.MediaService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.MediaService.UnitTests;

public class EndCallCommandHandlerTests
{
    private readonly Mock<IMediaUnitOfWork> _uowMock = new();
    private readonly Mock<ICallSessionRepository> _callRepoMock = new();
    private readonly Mock<ICallNotificationService> _notificationServiceMock = new();

    public EndCallCommandHandlerTests()
    {
        _uowMock.Setup(u => u.CallSessions).Returns(_callRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task EndCall_DirectCall_ShouldEndSessionAndCalculateDuration()
    {
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

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.Status.Should().Be(CallStatus.Ended);
        result.Data.DurationSeconds.Should().BeGreaterThan(290);
        result.Data.EndReason.Should().Be("Call completed normally");

        _notificationServiceMock.Verify(n => n.NotifyCallEndedAsync(callId, "Call completed normally", It.IsAny<int>()), Times.Once);
    }
}
