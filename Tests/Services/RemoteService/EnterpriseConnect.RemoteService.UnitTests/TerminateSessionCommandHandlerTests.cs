using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.TerminateSession;
using EnterpriseConnect.RemoteService.Domain.Entities;
using EnterpriseConnect.RemoteService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.RemoteService.UnitTests;

public class TerminateSessionCommandHandlerTests
{
    private readonly Mock<IRemoteUnitOfWork> _uowMock = new();
    private readonly Mock<IRemoteSessionRepository> _sessionRepoMock = new();
    private readonly Mock<IRemoteNotificationService> _notificationMock = new();

    public TerminateSessionCommandHandlerTests()
    {
        _uowMock.Setup(u => u.RemoteSessions).Returns(_sessionRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task TerminateSession_ShouldEndSessionAndCalculateDuration()
    {
        var sessionId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var supporterId = Guid.NewGuid();
        var connectedTime = DateTimeOffset.UtcNow.AddMinutes(-10);

        var session = new RemoteSession
        {
            Id = sessionId,
            RequesterId = requesterId,
            SupporterId = supporterId,
            Status = RemoteSessionStatus.Active,
            ConnectedAt = connectedTime,
            AccessGrants = new List<AccessGrant>
            {
                new() { SessionId = sessionId, PermissionType = PermissionType.ViewScreen, IsGranted = true }
            }
        };

        _sessionRepoMock.Setup(r => r.GetByIdWithGrantsAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new TerminateSessionCommandHandler(_uowMock.Object, _notificationMock.Object);
        var command = new TerminateSessionCommand(sessionId, requesterId, "Issue solved");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.Status.Should().Be(RemoteSessionStatus.Terminated);
        result.Data.DurationSeconds.Should().BeGreaterThan(590);
        result.Data.EndReason.Should().Be("Issue solved");

        _notificationMock.Verify(n => n.NotifySessionTerminatedAsync(sessionId, "Issue solved"), Times.Once);
    }
}
