using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.ApproveSession;
using EnterpriseConnect.RemoteService.Domain.Entities;
using EnterpriseConnect.RemoteService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.RemoteService.UnitTests;

public class ApproveSessionCommandHandlerTests
{
    private readonly Mock<IRemoteUnitOfWork> _uowMock = new();
    private readonly Mock<IRemoteSessionRepository> _sessionRepoMock = new();
    private readonly Mock<IRemoteNotificationService> _notificationMock = new();

    public ApproveSessionCommandHandlerTests()
    {
        _uowMock.Setup(u => u.RemoteSessions).Returns(_sessionRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task ApproveSession_Accept_ShouldTransitionToActiveAndGrantView()
    {
        var sessionId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var supporterId = Guid.NewGuid();

        var session = new RemoteSession
        {
            Id = sessionId,
            RequesterId = requesterId,
            SupporterId = supporterId,
            Status = RemoteSessionStatus.PendingApproval,
            AccessGrants = new List<AccessGrant>
            {
                new() { SessionId = sessionId, PermissionType = PermissionType.ViewScreen, IsGranted = false }
            }
        };

        _sessionRepoMock.Setup(r => r.GetByIdWithGrantsAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new ApproveSessionCommandHandler(_uowMock.Object, _notificationMock.Object);
        var command = new ApproveSessionCommand(sessionId, requesterId, Accept: true);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.Status.Should().Be(RemoteSessionStatus.Active);
        result.Data.ConnectedAt.Should().NotBeNull();
        session.AccessGrants.First(g => g.PermissionType == PermissionType.ViewScreen).IsGranted.Should().BeTrue();

        _notificationMock.Verify(n => n.NotifyConnectionApprovedAsync(supporterId, It.IsAny<RemoteSessionDto>()), Times.Once);
    }

    [Fact]
    public async Task ApproveSession_Reject_ShouldTransitionToRejected()
    {
        var sessionId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var supporterId = Guid.NewGuid();

        var session = new RemoteSession
        {
            Id = sessionId,
            RequesterId = requesterId,
            SupporterId = supporterId,
            Status = RemoteSessionStatus.PendingApproval
        };

        _sessionRepoMock.Setup(r => r.GetByIdWithGrantsAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new ApproveSessionCommandHandler(_uowMock.Object, _notificationMock.Object);
        var command = new ApproveSessionCommand(sessionId, requesterId, Accept: false, RejectionReason: "Busy right now");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.Status.Should().Be(RemoteSessionStatus.Rejected);
        result.Data.EndReason.Should().Be("Busy right now");

        _notificationMock.Verify(n => n.NotifyConnectionRejectedAsync(supporterId, "Busy right now"), Times.Once);
    }
}
