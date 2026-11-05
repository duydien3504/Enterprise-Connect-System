using EnterpriseConnect.MediaService.Domain.Enums;

namespace EnterpriseConnect.MediaService.Application.Common.Models;

public class CallSessionDto
{
    public Guid Id { get; set; }
    public Guid? ConversationId { get; set; }
    public Guid InitiatorId { get; set; }
    public CallType Type { get; set; }
    public CallScope Scope { get; set; }
    public CallStatus Status { get; set; }
    public string? LiveKitRoomName { get; set; }
    public string? LiveKitToken { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? ConnectedAt { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public int DurationSeconds { get; set; }
    public string? EndReason { get; set; }
    public List<CallParticipantDto> Participants { get; set; } = new();
}
