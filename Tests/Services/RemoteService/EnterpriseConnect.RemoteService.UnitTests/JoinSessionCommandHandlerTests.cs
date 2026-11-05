using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.JoinSession;
using EnterpriseConnect.RemoteService.Domain.Entities;
using EnterpriseConnect.RemoteService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.RemoteService.UnitTests;

public class JoinSessionCommandHandlerTests
{
    private readonly Mock<IRemoteUnitOfWork> _uowMock = new();
    private readonly Mock<IRemoteSessionRepository> _sessionRepoMock = new();
    private readonly Mock<IRemoteNotificationService> _notificationMock = new();

    public JoinSessionCommandHandlerTests()
    {
        _uowMock.Setup(u => u.RemoteSessions).Returns(_sessionRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task JoinSession_ValidCode_ShouldTransitionToPendingApproval()
    {
        var requesterId = Guid.NewGuid();
        var supporterId = Guid.NewGuid();
        var session = new RemoteSession
        {
            Id = Guid.NewGuid(),
            SessionCode = "842-195",
            RequesterId = requesterId,
            Status = RemoteSessionStatus.Created,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        };

        _sessionRepoMock.Setup(r => r.GetByCodeWithGrantsAsync("842-195", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new JoinSessionCommandHandler(_uowMock.Object, _notificationMock.Object);
        var command = new JoinSessionCommand("842-195", supporterId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.Status.Should().Be(RemoteSessionStatus.PendingApproval);
        result.Data.SupporterId.Should().Be(supporterId);

        _notificationMock.Verify(n => n.NotifyConnectionRequestedAsync(requesterId, supporterId, "842-195"), Times.Once);
    }

    [Fact]
    public async Task JoinSession_WhenRequesterTriesToSupportThemselves_ShouldFail()
    {
        var userId = Guid.NewGuid();
        var session = new RemoteSession
        {
            Id = Guid.NewGuid(),
            SessionCode = "842-195",
            RequesterId = userId,
            Status = RemoteSessionStatus.Created,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15)
        };

        _sessionRepoMock.Setup(r => r.GetByCodeWithGrantsAsync("842-195", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new JoinSessionCommandHandler(_uowMock.Object, _notificationMock.Object);
        var command = new JoinSessionCommand("842-195", userId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("cannot join your own");
    }
}
