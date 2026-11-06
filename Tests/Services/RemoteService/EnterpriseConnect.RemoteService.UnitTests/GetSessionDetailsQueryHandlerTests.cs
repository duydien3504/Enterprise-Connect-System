using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Queries.GetSessionDetails;
using EnterpriseConnect.RemoteService.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.RemoteService.UnitTests;

public class GetSessionDetailsQueryHandlerTests
{
    private readonly Mock<IRemoteUnitOfWork> _mockUow;
    private readonly GetSessionDetailsQueryHandler _handler;

    public GetSessionDetailsQueryHandlerTests()
    {
        _mockUow = new Mock<IRemoteUnitOfWork>();
        _handler = new GetSessionDetailsQueryHandler(_mockUow.Object);
    }

    [Fact]
    public async Task Handle_WhenCallerIsRequester_ShouldSucceed()
    {
        var sessionId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var session = new RemoteSession
        {
            Id = sessionId,
            RequesterId = requesterId,
            SessionCode = "ABC12345"
        };

        _mockUow.Setup(u => u.RemoteSessions.GetByIdWithGrantsAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var query = new GetSessionDetailsQuery(sessionId, requesterId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.SessionCode.Should().Be("ABC12345");
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotParticipant_ShouldDenyAccess()
    {
        var sessionId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var session = new RemoteSession
        {
            Id = sessionId,
            RequesterId = requesterId,
            SupporterId = Guid.NewGuid(),
            SessionCode = "ABC12345"
        };

        _mockUow.Setup(u => u.RemoteSessions.GetByIdWithGrantsAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var query = new GetSessionDetailsQuery(sessionId, attackerId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Access denied");
    }
}
