namespace EnterpriseConnect.MediaService.Api.Hubs;

public interface ICallClient
{
    Task IncomingCall(object callSession);
    Task CallAnswered(Guid callId, Guid participantId, object callSession);
    Task CallDeclined(Guid callId, Guid participantId, string reason);
    Task CallEnded(Guid callId, string reason, int durationSeconds);
    Task ParticipantJoined(Guid callId, object participant);
    Task ParticipantLeft(Guid callId, Guid participantId);
    Task MediaStateChanged(Guid callId, Guid participantId, bool? isMuted, bool? isCameraOn, bool? isScreenSharing);
    Task ReceiveSignalingMessage(string type, object payload);
}
