using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.AuditLogs.Commands.RecordAuditLog;

public class RecordAuditLogCommand : IRequest<Result<AuditLogDto>>
{
    public string CorrelationId { get; set; } = string.Empty;
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
    public int StatusCode { get; set; } = 200;
    public long ExecutionDurationMs { get; set; }
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Safe;
    public string? Details { get; set; }
    public string? FailureReason { get; set; }
}
