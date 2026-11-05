using EnterpriseConnect.MediaService.Domain.Enums;

namespace EnterpriseConnect.MediaService.Application.Common.Models;

public class CallParticipantDto
{
    public Guid Id { get; set; }
    public Guid CallId { get; set; }
    public Guid UserId { get; set; }
    public ParticipantStatus Status { get; set; }
    public bool IsMuted { get; set; }
    public bool IsCameraOn { get; set; }
    public bool IsScreenSharing { get; set; }
    public DateTimeOffset? JoinTime { get; set; }
    public DateTimeOffset? LeaveTime { get; set; }
}
