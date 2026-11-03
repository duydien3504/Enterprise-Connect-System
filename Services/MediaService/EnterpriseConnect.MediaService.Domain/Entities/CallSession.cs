using EnterpriseConnect.BuildingBlocks.Domain.Common;
using EnterpriseConnect.MediaService.Domain.Enums;

namespace EnterpriseConnect.MediaService.Domain.Entities;

public class CallSession : BaseEntity
{
    public Guid? ConversationId { get; set; } // Logical FK to MessagingService.Conversation
    public Guid InitiatorId { get; set; } // Logical FK to IdentityService.User
    public CallType Type { get; set; } = CallType.Voice;
    public CallStatus Status { get; set; } = CallStatus.Initiated;
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? EndTime { get; set; }

    public virtual ICollection<CallParticipant> Participants { get; set; } = new List<CallParticipant>();
}
