using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Features.AuditLogs.Commands.RecordAuditLog;
using EnterpriseConnect.AdminService.Domain.Entities;
using EnterpriseConnect.AdminService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.AdminService.UnitTests;

public class RecordAuditLogCommandHandlerTests
{
    private readonly Mock<IAdminUnitOfWork> _uowMock = new();
    private readonly Mock<IAuditLogRepository> _repoMock = new();
    private readonly Mock<IAuditLogDeduplicator> _dedupMock = new();
    private readonly RecordAuditLogCommandHandler _handler;

    public RecordAuditLogCommandHandlerTests()
    {
        _uowMock.Setup(u => u.AuditLogs).Returns(_repoMock.Object);
        _handler = new RecordAuditLogCommandHandler(_uowMock.Object, _dedupMock.Object);
    }

    [Fact]
    public async Task Handle_ValidLog_ShouldSaveAndReturnSuccess()
    {
        var command = new RecordAuditLogCommand
        {
            CorrelationId = "corr-100",
            ServiceName = "MessagingService",
            ActionName = "SendMessage",
            TargetEntity = "Message",
            ActorId = Guid.NewGuid(),
            IpAddress = "127.0.0.1",
            HttpMethod = "POST",
            EndpointUrl = "/api/messages",
            StatusCode = 200,
            RiskLevel = RiskLevel.Safe
        };

        _dedupMock.Setup(d => d.IsDuplicate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CorrelationId.Should().Be("corr-100");
        _repoMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateLog_ShouldSuppressAndNotSave()
    {
        var command = new RecordAuditLogCommand
        {
            CorrelationId = "corr-dup",
            ServiceName = "MessagingService",
            ActionName = "SendMessage",
            TargetEntity = "Message",
            ActorId = Guid.NewGuid(),
            IpAddress = "127.0.0.1",
            HttpMethod = "POST",
            EndpointUrl = "/api/messages"
        };

        _dedupMock.Setup(d => d.IsDuplicate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Duplicate");
        _repoMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
