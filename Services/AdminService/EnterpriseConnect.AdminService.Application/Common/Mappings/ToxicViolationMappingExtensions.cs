using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Entities;

namespace EnterpriseConnect.AdminService.Application.Common.Mappings;

public static class ToxicViolationMappingExtensions
{
    public static ToxicViolationDto ToDto(this ToxicViolation entity)
    {
        return new ToxicViolationDto
        {
            Id = entity.Id,
            MessageId = entity.MessageId,
            SenderId = entity.SenderId,
            ViolationType = entity.ViolationType,
            Severity = entity.Severity,
            ActionTaken = entity.ActionTaken,
            DetectedAt = entity.DetectedAt
        };
    }
}
