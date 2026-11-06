namespace EnterpriseConnect.BuildingBlocks.Application.Audit;

public interface IAuditClient
{
    Task RecordEventAsync(AuditEventDto auditEvent, CancellationToken cancellationToken = default);
}
