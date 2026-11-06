namespace EnterpriseConnect.AdminService.Application.Common.Interfaces;

public interface IAuditLogDeduplicator
{
    bool IsDuplicate(string correlationId, string serviceName, string actionName, Guid actorId, string endpointUrl);
}
