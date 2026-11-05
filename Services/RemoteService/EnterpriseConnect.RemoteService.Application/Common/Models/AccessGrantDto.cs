using EnterpriseConnect.RemoteService.Domain.Enums;

namespace EnterpriseConnect.RemoteService.Application.Common.Models;

public class AccessGrantDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public PermissionType PermissionType { get; set; }
    public bool IsGranted { get; set; }
    public DateTimeOffset? GrantedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}
