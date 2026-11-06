using EnterpriseConnect.AdminService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace EnterpriseConnect.AdminService.UnitTests;

public class AuditLogDeduplicatorTests
{
    private readonly AuditLogDeduplicator _deduplicator;

    public AuditLogDeduplicatorTests()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        _deduplicator = new AuditLogDeduplicator(memoryCache);
    }

    [Fact]
    public void IsDuplicate_FirstCall_ShouldReturnFalse()
    {
        var actorId = Guid.NewGuid();
        var result = _deduplicator.IsDuplicate("corr-1", "Gateway", "Login", actorId, "/api/auth/login");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsDuplicate_SecondCallWithinWindow_ShouldReturnTrue()
    {
        var actorId = Guid.NewGuid();
        _deduplicator.IsDuplicate("corr-2", "Gateway", "Login", actorId, "/api/auth/login");
        var second = _deduplicator.IsDuplicate("corr-2", "Gateway", "Login", actorId, "/api/auth/login");
        second.Should().BeTrue();
    }

    [Fact]
    public void IsDuplicate_DifferentCorrelationId_ShouldReturnFalse()
    {
        var actorId = Guid.NewGuid();
        _deduplicator.IsDuplicate("corr-3", "Gateway", "Login", actorId, "/api/auth/login");
        var different = _deduplicator.IsDuplicate("corr-4", "Gateway", "Login", actorId, "/api/auth/login");
        different.Should().BeFalse();
    }
}
