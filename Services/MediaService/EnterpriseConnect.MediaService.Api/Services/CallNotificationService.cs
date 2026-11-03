using EnterpriseConnect.MediaService.Api.Hubs;
using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace EnterpriseConnect.MediaService.Api.Services;

public class CallNotificationService : ICallNotificationService
{
    private readonly IHubContext<CallHub, ICallClient> _hubContext;

    public CallNotificationService(IHubContext<CallHub, ICallClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyIncomingCallAsync(Guid targetUserId, object callDto)
    {
        await _hubContext.Clients.User(targetUserId.ToString()).IncomingCall(callDto);
    }

    public async Task NotifyCallAnsweredAsync(Guid callId, Guid participantId, object callDto)
    {
        await _hubContext.Clients.Group(callId.ToString()).CallAnswered(callId, participantId, callDto);
    }

    public async Task NotifyCallDeclinedAsync(Guid callId, Guid participantId, string reason)
    {
        await _hubContext.Clients.Group(callId.ToString()).CallDeclined(callId, participantId, reason);
    }

    public async Task NotifyCallEndedAsync(Guid callId, string reason, int durationSeconds)
    {
        await _hubContext.Clients.Group(callId.ToString()).CallEnded(callId, reason, durationSeconds);
    }

    public async Task NotifyParticipantJoinedAsync(Guid callId, object participantDto)
    {
        await _hubContext.Clients.Group(callId.ToString()).ParticipantJoined(callId, participantDto);
    }

    public async Task NotifyParticipantLeftAsync(Guid callId, Guid participantId)
    {
        await _hubContext.Clients.Group(callId.ToString()).ParticipantLeft(callId, participantId);
    }

    public async Task NotifyMediaStateChangedAsync(Guid callId, Guid participantId, bool? isMuted, bool? isCameraOn, bool? isScreenSharing)
    {
        await _hubContext.Clients.Group(callId.ToString()).MediaStateChanged(callId, participantId, isMuted, isCameraOn, isScreenSharing);
    }

    public async Task SendSignalingMessageAsync(Guid targetUserId, string type, object payload)
    {
        await _hubContext.Clients.User(targetUserId.ToString()).ReceiveSignalingMessage(type, payload);
    }
}
