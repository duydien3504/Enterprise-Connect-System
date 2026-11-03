using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.EndCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.RespondCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.UpdateMediaState;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace EnterpriseConnect.MediaService.Api.Hubs;

public class CallHub : Hub<ICallClient>
{
    private readonly IMediator _mediator;

    public CallHub(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task JoinCallRoom(Guid callId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, callId.ToString());
    }

    public async Task LeaveCallRoom(Guid callId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, callId.ToString());
    }

    public async Task RespondCall(Guid callId, Guid userId, bool accept, string? declineReason = null)
    {
        await JoinCallRoom(callId);
        await _mediator.Send(new RespondCallCommand(callId, userId, accept, declineReason));
    }

    public async Task EndCall(Guid callId, Guid userId, string? reason = null)
    {
        await _mediator.Send(new EndCallCommand(callId, userId, reason));
        await LeaveCallRoom(callId);
    }

    public async Task UpdateMediaState(Guid callId, Guid userId, bool? isMuted, bool? isCameraOn, bool? isScreenSharing)
    {
        await _mediator.Send(new UpdateMediaStateCommand(callId, userId, isMuted, isCameraOn, isScreenSharing));
    }

    // WebRTC P2P Signaling Relay (Offer, Answer, Candidate)
    public async Task SendSignaling(Guid targetUserId, string type, object payload)
    {
        await Clients.User(targetUserId.ToString()).ReceiveSignalingMessage(type, payload);
    }
}
