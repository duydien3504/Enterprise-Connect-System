using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Entities;

namespace EnterpriseConnect.AdminService.Application.Common.Mappings;

public static class AuditLogMappingExtensions
{
    public static AuditLogDto ToDto(this AuditLog entity)
    {
        return new AuditLogDto
        {
            Id = entity.Id,
            CorrelationId = entity.CorrelationId,
            Timestamp = entity.Timestamp,
            ServiceName = entity.ServiceName,
            ActionName = entity.ActionName,
            TargetEntity = entity.TargetEntity,
            TargetId = entity.TargetId,
            ActorId = entity.ActorId,
            ActorEmail = entity.ActorEmail,
            ActorRole = entity.ActorRole,
            IpAddress = entity.IpAddress,
            UserAgent = entity.UserAgent,
            HttpMethod = entity.HttpMethod,
            EndpointUrl = entity.EndpointUrl,
            StatusCode = entity.StatusCode,
            ExecutionDurationMs = entity.ExecutionDurationMs,
            RiskLevel = entity.RiskLevel,
            Details = entity.Details,
            FailureReason = entity.FailureReason
        };
    }
}
