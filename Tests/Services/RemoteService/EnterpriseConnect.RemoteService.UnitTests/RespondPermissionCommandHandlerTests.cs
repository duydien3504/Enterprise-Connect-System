using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RespondPermission;
using EnterpriseConnect.RemoteService.Domain.Entities;
using EnterpriseConnect.RemoteService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.RemoteService.UnitTests;

public class RespondPermissionCommandHandlerTests
{
    private readonly Mock<IRemoteUnitOfWork> _uowMock = new();
    private readonly Mock<IRemoteSessionRepository> _sessionRepoMock = new();
    private readonly Mock<IRemoteNotificationService> _notificationMock = new();

    public RespondPermissionCommandHandlerTests()
    {
        _uowMock.Setup(u => u.RemoteSessions).Returns(_sessionRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task RespondPermission_Grant_ShouldSetIsGrantedTrueAndNotifySupporter()
    {
        var sessionId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var supporterId = Guid.NewGuid();

        var session = new RemoteSession
        {
            Id = sessionId,
            RequesterId = requesterId,
            SupporterId = supporterId,
            Status = RemoteSessionStatus.Active,
            AccessGrants = new List<AccessGrant>
            {
                new() { SessionId = sessionId, PermissionType = PermissionType.ControlInput, IsGranted = false }
            }
        };

        _sessionRepoMock.Setup(r => r.GetByIdWithGrantsAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var handler = new RespondPermissionCommandHandler(_uowMock.Object, _notificationMock.Object);
        var command = new RespondPermissionCommand(sessionId, requesterId, PermissionType.ControlInput, Grant: true);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.IsGranted.Should().BeTrue();
        result.Data.GrantedAt.Should().NotBeNull();

        _notificationMock.Verify(n => n.NotifyPermissionUpdatedAsync(supporterId, PermissionType.ControlInput, true), Times.Once);
    }
}
