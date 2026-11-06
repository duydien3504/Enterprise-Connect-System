namespace EnterpriseConnect.BuildingBlocks.Application.Audit;

public class AuditEventDto
{
    public string CorrelationId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public Guid ActorId { get; set; }
    public string? ActorEmail { get; set; }
    public string? ActorRole { get; set; }
    public string IpAddress { get; set; } = "127.0.0.1";
    public string UserAgent { get; set; } = "internal-service";
    public string HttpMethod { get; set; } = "POST";
    public string EndpointUrl { get; set; } = string.Empty;
    public int StatusCode { get; set; } = 200;
    public long ExecutionDurationMs { get; set; }
    public int RiskLevel { get; set; }
    public string? Details { get; set; }
    public string? FailureReason { get; set; }
}
