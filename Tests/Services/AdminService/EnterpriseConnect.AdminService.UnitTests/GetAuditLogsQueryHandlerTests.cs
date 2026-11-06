using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Features.AuditLogs.Queries.GetAuditLogs;
using EnterpriseConnect.AdminService.Domain.Entities;
using EnterpriseConnect.AdminService.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.AdminService.UnitTests;

public class GetAuditLogsQueryHandlerTests
{
    private readonly Mock<IAdminUnitOfWork> _uowMock = new();
    private readonly Mock<IAuditLogRepository> _repoMock = new();
    private readonly GetAuditLogsQueryHandler _handler;

    public GetAuditLogsQueryHandlerTests()
    {
        _uowMock.Setup(u => u.AuditLogs).Returns(_repoMock.Object);
        _handler = new GetAuditLogsQueryHandler(_uowMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsPagedAuditLogs()
    {
        var logs = new List<AuditLog>
        {
            new() { Id = Guid.NewGuid(), CorrelationId = "c1", ServiceName = "Gateway", RiskLevel = RiskLevel.Safe },
            new() { Id = Guid.NewGuid(), CorrelationId = "c2", ServiceName = "Identity", RiskLevel = RiskLevel.Danger }
        };

        _repoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<RiskLevel?>(),
            It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>(),
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((logs, 2L));

        var query = new GetAuditLogsQuery { Page = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data.TotalCount.Should().Be(2);
        result.Data.Items.Should().HaveCount(2);
        result.Data.Items[0].ServiceName.Should().Be("Gateway");
    }
}
