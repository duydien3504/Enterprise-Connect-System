using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Domain.Entities;

namespace EnterpriseConnect.RemoteService.Application.Common.Mappings;

public static class RemoteSessionMappingExtensions
{
    public static RemoteSessionDto ToDto(this RemoteSession session)
    {
        return new RemoteSessionDto
        {
            Id = session.Id,
            SessionCode = session.SessionCode,
            RequesterId = session.RequesterId,
            SupporterId = session.SupporterId,
            Status = session.Status,
            ExpiresAt = session.ExpiresAt,
            ConnectedAt = session.ConnectedAt,
            EndedAt = session.EndedAt,
            DurationSeconds = session.DurationSeconds,
            EndReason = session.EndReason,
            AccessGrants = session.AccessGrants.Select(g => g.ToDto()).ToList()
        };
    }

    public static AccessGrantDto ToDto(this AccessGrant grant)
    {
        return new AccessGrantDto
        {
            Id = grant.Id,
            SessionId = grant.SessionId,
            PermissionType = grant.PermissionType,
            IsGranted = grant.IsGranted,
            GrantedAt = grant.GrantedAt,
            RevokedAt = grant.RevokedAt
        };
    }
}
