using System.Security.Claims;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.EndCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.RespondCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.UpdateMediaState;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EnterpriseConnect.MediaService.Api.Hubs;

[Authorize]
public class CallHub : Hub<ICallClient>
{
    private readonly IMediator _mediator;

    public CallHub(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid CurrentUserId
    {
        get
        {
            var idStr = Context.UserIdentifier
                ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? Context.User?.FindFirst("sub")?.Value;

            return Guid.TryParse(idStr, out var id)
                ? id
                : throw new HubException("User is not authenticated.");
        }
    }

    public async Task JoinCallRoom(Guid callId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, callId.ToString());
    }

    public async Task LeaveCallRoom(Guid callId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, callId.ToString());
    }

    public async Task RespondCall(Guid callId, bool accept, string? declineReason = null)
    {
        await JoinCallRoom(callId);
        await _mediator.Send(new RespondCallCommand(callId, CurrentUserId, accept, declineReason));
    }

    public async Task EndCall(Guid callId, string? reason = null)
    {
        await _mediator.Send(new EndCallCommand(callId, CurrentUserId, reason));
        await LeaveCallRoom(callId);
    }

    public async Task UpdateMediaState(Guid callId, bool? isMuted, bool? isCameraOn, bool? isScreenSharing)
    {
        await _mediator.Send(new UpdateMediaStateCommand(callId, CurrentUserId, isMuted, isCameraOn, isScreenSharing));
    }

    public async Task SendSignaling(Guid targetUserId, string type, object payload)
    {
        await Clients.User(targetUserId.ToString()).ReceiveSignalingMessage(type, payload);
    }
}
