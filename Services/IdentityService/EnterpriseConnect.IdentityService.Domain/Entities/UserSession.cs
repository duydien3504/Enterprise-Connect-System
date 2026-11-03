using EnterpriseConnect.BuildingBlocks.Domain.Common;
using EnterpriseConnect.IdentityService.Domain.Enums;

namespace EnterpriseConnect.IdentityService.Domain.Entities;

public class UserSession : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DeviceType DeviceType { get; set; } = DeviceType.Web;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public virtual User User { get; set; } = null!;
}
