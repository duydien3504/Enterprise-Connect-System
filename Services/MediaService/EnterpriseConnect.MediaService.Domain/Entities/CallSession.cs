using EnterpriseConnect.BuildingBlocks.Domain.Common;
using EnterpriseConnect.MediaService.Domain.Enums;

namespace EnterpriseConnect.MediaService.Domain.Entities;

public class CallSession : BaseEntity
{
    public Guid? ConversationId { get; set; } // Logical FK to MessagingService.Conversation
    public Guid InitiatorId { get; set; } // Logical FK to IdentityService.User
    public CallType Type { get; set; } = CallType.Voice;
    public CallScope Scope { get; set; } = CallScope.Direct;
    public CallStatus Status { get; set; } = CallStatus.Initiated;
    public string? LiveKitRoomName { get; set; }
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ConnectedAt { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public int DurationSeconds { get; set; } = 0;
    public string? EndReason { get; set; }

    public virtual ICollection<CallParticipant> Participants { get; set; } = new List<CallParticipant>();
}
