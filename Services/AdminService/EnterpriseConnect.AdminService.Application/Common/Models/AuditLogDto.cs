using EnterpriseConnect.AdminService.Domain.Enums;

namespace EnterpriseConnect.AdminService.Application.Common.Models;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public Guid ActorId { get; set; }
    public string? ActorEmail { get; set; }
    public string? ActorRole { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string EndpointUrl { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ExecutionDurationMs { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public string? Details { get; set; }
    public string? FailureReason { get; set; }

    public string BackgroundColor => RiskLevel switch
    {
        RiskLevel.Safe => "#ecfdf5",
        RiskLevel.Warning => "#fffbeb",
        RiskLevel.Danger => "#fef2f2",
        _ => "#f8fafc"
    };

    public string TextColor => RiskLevel switch
    {
        RiskLevel.Safe => "#166534",
        RiskLevel.Warning => "#854d0e",
        RiskLevel.Danger => "#991b1b",
        _ => "#334155"
    };

    public string BorderColor => RiskLevel switch
    {
        RiskLevel.Safe => "#22c55e",
        RiskLevel.Warning => "#f59e0b",
        RiskLevel.Danger => "#ef4444",
        _ => "#cbd5e1"
    };

    public string Badge => RiskLevel switch
    {
        RiskLevel.Safe => "safe",
        RiskLevel.Warning => "warning",
        RiskLevel.Danger => "danger",
        _ => "default"
    };

    public string Icon => RiskLevel switch
    {
        RiskLevel.Safe => "check-circle",
        RiskLevel.Warning => "alert-triangle",
        RiskLevel.Danger => "shield-alert",
        _ => "info"
    };
}
