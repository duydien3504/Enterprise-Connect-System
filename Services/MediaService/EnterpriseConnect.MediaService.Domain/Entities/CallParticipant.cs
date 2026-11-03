using EnterpriseConnect.BuildingBlocks.Domain.Common;
using EnterpriseConnect.MediaService.Domain.Enums;

namespace EnterpriseConnect.MediaService.Domain.Entities;

public class CallParticipant : BaseEntity
{
    public Guid CallId { get; set; }
    public Guid UserId { get; set; } // Logical FK to IdentityService.User
    public ParticipantStatus Status { get; set; } = ParticipantStatus.Inviting;
    public bool IsMuted { get; set; } = false;
    public bool IsCameraOn { get; set; } = false;
    public bool IsScreenSharing { get; set; } = false;
    public DateTimeOffset? JoinTime { get; set; }
    public DateTimeOffset? LeaveTime { get; set; }

    public virtual CallSession CallSession { get; set; } = null!;
}
