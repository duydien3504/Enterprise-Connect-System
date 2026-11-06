using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Features.System.Commands.UpdateSystemSetting;
using EnterpriseConnect.AdminService.Application.Features.System.Queries.GetDashboardOverview;
using EnterpriseConnect.AdminService.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnterpriseConnect.AdminService.UnitTests;

public class SystemCommandHandlerTests
{
    private readonly Mock<IAdminUnitOfWork> _uowMock = new();
    private readonly Mock<ISystemSettingRepository> _settingRepoMock = new();
    private readonly Mock<IAuditLogRepository> _auditRepoMock = new();
    private readonly Mock<IDocumentRepository> _docRepoMock = new();
    private readonly Mock<IToxicViolationRepository> _violationRepoMock = new();

    public SystemCommandHandlerTests()
    {
        _uowMock.Setup(u => u.SystemSettings).Returns(_settingRepoMock.Object);
        _uowMock.Setup(u => u.AuditLogs).Returns(_auditRepoMock.Object);
        _uowMock.Setup(u => u.Documents).Returns(_docRepoMock.Object);
        _uowMock.Setup(u => u.ToxicViolations).Returns(_violationRepoMock.Object);
    }

    [Fact]
    public async Task UpdateSetting_NewKey_ShouldAddAndSave()
    {
        _settingRepoMock.Setup(s => s.GetByKeyAsync("MaxUploadSize", It.IsAny<CancellationToken>()))
            .ReturnsAsync((SystemSetting?)null);

        var handler = new UpdateSystemSettingCommandHandler(_uowMock.Object);
        var command = new UpdateSystemSettingCommand
        {
            SettingKey = "MaxUploadSize",
            SettingValue = "104857600",
            GroupName = "Storage"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.SettingKey.Should().Be("MaxUploadSize");
        _settingRepoMock.Verify(s => s.AddOrUpdateAsync(It.IsAny<SystemSetting>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDashboard_ShouldAggregateAllMetrics()
    {
        _auditRepoMock.Setup(a => a.GetStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((100L, 80L, 15L, 5L, new List<string> { "IdentityService", "MessagingService" }));
        _docRepoMock.Setup(d => d.GetTotalCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(50L);
        _docRepoMock.Setup(d => d.GetIndexedCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(45L);
        _violationRepoMock.Setup(v => v.GetTotalCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(10L);
        _violationRepoMock.Setup(v => v.GetPendingReviewCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2L);

        var handler = new GetDashboardOverviewQueryHandler(_uowMock.Object);
        var result = await handler.Handle(new GetDashboardOverviewQuery(), CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Data!.TotalAuditLogs.Should().Be(100);
        result.Data.SafeLogsCount.Should().Be(80);
        result.Data.WarningLogsCount.Should().Be(15);
        result.Data.DangerLogsCount.Should().Be(5);
        result.Data.TotalDocuments.Should().Be(50);
        result.Data.IndexedDocuments.Should().Be(45);
        result.Data.TotalViolations.Should().Be(10);
        result.Data.PendingViolations.Should().Be(2);
        result.Data.ActiveServices.Should().Contain("IdentityService");
    }
}
