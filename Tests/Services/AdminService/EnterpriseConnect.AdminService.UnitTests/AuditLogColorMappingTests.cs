using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EnterpriseConnect.AdminService.UnitTests;

public class AuditLogColorMappingTests
{
    [Fact]
    public void AuditLogDto_SafeRisk_ShouldReturnGreenPalette()
    {
        var dto = new AuditLogDto { RiskLevel = RiskLevel.Safe };
        dto.BackgroundColor.Should().Be("#ecfdf5");
        dto.TextColor.Should().Be("#166534");
        dto.BorderColor.Should().Be("#22c55e");
        dto.Badge.Should().Be("safe");
        dto.Icon.Should().Be("check-circle");
    }

    [Fact]
    public void AuditLogDto_WarningRisk_ShouldReturnGoldAmberPalette()
    {
        var dto = new AuditLogDto { RiskLevel = RiskLevel.Warning };
        dto.BackgroundColor.Should().Be("#fffbeb");
        dto.TextColor.Should().Be("#854d0e");
        dto.BorderColor.Should().Be("#f59e0b");
        dto.Badge.Should().Be("warning");
        dto.Icon.Should().Be("alert-triangle");
    }

    [Fact]
    public void AuditLogDto_DangerRisk_ShouldReturnRedPalette()
    {
        var dto = new AuditLogDto { RiskLevel = RiskLevel.Danger };
        dto.BackgroundColor.Should().Be("#fef2f2");
        dto.TextColor.Should().Be("#991b1b");
        dto.BorderColor.Should().Be("#ef4444");
        dto.Badge.Should().Be("danger");
        dto.Icon.Should().Be("shield-alert");
    }
}
