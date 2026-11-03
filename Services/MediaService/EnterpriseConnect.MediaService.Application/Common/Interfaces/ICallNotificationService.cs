namespace EnterpriseConnect.MediaService.Application.Common.Interfaces;

public interface ICallNotificationService
{
    Task NotifyIncomingCallAsync(Guid targetUserId, object callDto);
    Task NotifyCallAnsweredAsync(Guid callId, Guid participantId, object callDto);
    Task NotifyCallDeclinedAsync(Guid callId, Guid participantId, string reason);
    Task NotifyCallEndedAsync(Guid callId, string reason, int durationSeconds);
    Task NotifyParticipantJoinedAsync(Guid callId, object participantDto);
    Task NotifyParticipantLeftAsync(Guid callId, Guid participantId);
    Task NotifyMediaStateChangedAsync(Guid callId, Guid participantId, bool? isMuted, bool? isCameraOn, bool? isScreenSharing);
    Task SendSignalingMessageAsync(Guid targetUserId, string type, object payload);
}
