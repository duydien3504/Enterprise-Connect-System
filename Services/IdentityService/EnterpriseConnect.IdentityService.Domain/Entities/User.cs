using EnterpriseConnect.BuildingBlocks.Domain.Common;
using EnterpriseConnect.IdentityService.Domain.Enums;

namespace EnterpriseConnect.IdentityService.Domain.Entities;

public class User : BaseEntity
{
    public Guid? DepartmentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Position { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public bool EmailConfirmed { get; set; } = false;
    public string? LockReason { get; set; }
    public DateTimeOffset? LockedAt { get; set; }

    public virtual Department? Department { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    public virtual ICollection<Contact> SentContacts { get; set; } = new List<Contact>();
    public virtual ICollection<Contact> ReceivedContacts { get; set; } = new List<Contact>();
}
