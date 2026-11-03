using EnterpriseConnect.BuildingBlocks.Domain.Common;

namespace EnterpriseConnect.MediaService.Domain.Entities;

public class CallParticipant : BaseEntity
{
    public Guid CallId { get; set; }
    public Guid UserId { get; set; } // Logical FK to IdentityService.User
    public DateTimeOffset JoinTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LeaveTime { get; set; }

    public virtual CallSession CallSession { get; set; } = null!;
}
