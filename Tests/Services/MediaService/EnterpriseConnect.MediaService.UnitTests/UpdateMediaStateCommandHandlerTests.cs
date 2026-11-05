using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.UpdateMediaState;
using EnterpriseConnect.MediaService.Domain.Entities;
using EnterpriseConnect.MediaService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.MediaService.UnitTests;

public class UpdateMediaStateCommandHandlerTests
{
    private readonly Mock<IMediaUnitOfWork> _uowMock = new();
    private readonly Mock<ICallSessionRepository> _callRepoMock = new();
    private readonly Mock<ICallNotificationService> _notificationServiceMock = new();

    public UpdateMediaStateCommandHandlerTests()
    {
        _uowMock.Setup(u => u.CallSessions).Returns(_callRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task UpdateMediaState_ShouldUpdateParticipantFlags()
    {
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

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.IsMuted.Should().BeTrue();
        result.Data.IsCameraOn.Should().BeFalse();
        result.Data.IsScreenSharing.Should().BeTrue();

        _notificationServiceMock.Verify(n => n.NotifyMediaStateChangedAsync(callId, userId, true, false, true), Times.Once);
    }
}
