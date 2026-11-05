using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RequestPermission;
using EnterpriseConnect.RemoteService.Domain.Entities;
using EnterpriseConnect.RemoteService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.RemoteService.UnitTests;

public class RequestPermissionCommandHandlerTests
{
    private readonly Mock<IRemoteUnitOfWork> _uowMock = new();
    private readonly Mock<IRemoteSessionRepository> _sessionRepoMock = new();
    private readonly Mock<IRemoteNotificationService> _notificationMock = new();

    public RequestPermissionCommandHandlerTests()
    {
        _uowMock.Setup(u => u.RemoteSessions).Returns(_sessionRepoMock.Object);
    }

    [Fact]
    public async Task RequestPermission_ActiveSession_ShouldNotifyRequester()
    {
        var sessionId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var supporterId = Guid.NewGuid();

        var session = new RemoteSession
        {
            Id = sessionId,
            RequesterId = requesterId,
            SupporterId = supporterId,
            Status = RemoteSessionStatus.Active
        };

        _sessionRepoMock.Setup(r => r.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new RequestPermissionCommandHandler(_uowMock.Object, _notificationMock.Object);
        var command = new RequestPermissionCommand(sessionId, supporterId, PermissionType.ControlInput);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        _notificationMock.Verify(n => n.NotifyPermissionRequestedAsync(requesterId, PermissionType.ControlInput), Times.Once);
    }
}
