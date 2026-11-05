using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.CreateSession;
using EnterpriseConnect.RemoteService.Domain.Entities;
using EnterpriseConnect.RemoteService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.RemoteService.UnitTests;

public class CreateSessionCommandHandlerTests
{
    private readonly Mock<IRemoteUnitOfWork> _uowMock = new();
    private readonly Mock<IRemoteSessionRepository> _sessionRepoMock = new();
    private readonly Mock<ISessionCodeGenerator> _codeGenMock = new();

    public CreateSessionCommandHandlerTests()
    {
        _uowMock.Setup(u => u.RemoteSessions).Returns(_sessionRepoMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _codeGenMock.Setup(c => c.GenerateCode()).Returns("123-456");
    }

    [Fact]
    public async Task CreateSession_ShouldInitializeWithCreatedStatusAndGrants()
    {
        var requesterId = Guid.NewGuid();
        _sessionRepoMock.Setup(r => r.GetActiveSessionByRequesterAsync(requesterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RemoteSession?)null);

        var handler = new CreateSessionCommandHandler(_uowMock.Object, _codeGenMock.Object);
        var command = new CreateSessionCommand(requesterId, ExpirationMinutes: 15);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.SessionCode.Should().Be("123-456");
        result.Data.RequesterId.Should().Be(requesterId);
        result.Data.Status.Should().Be(RemoteSessionStatus.Created);
        result.Data.AccessGrants.Should().HaveCount(2);

        _sessionRepoMock.Verify(r => r.AddAsync(It.IsAny<RemoteSession>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
