using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EnterpriseConnect.RemoteService.Api.Hubs;

[Authorize]
public class RemoteHub : Hub<IRemoteClient>
{
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

    public async Task JoinSessionRoom(Guid sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId.ToString());
    }

    public async Task LeaveSessionRoom(Guid sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId.ToString());
    }

    public async Task SendSignaling(Guid targetUserId, string type, object payload)
    {
        await Clients.User(targetUserId.ToString()).ReceiveSignalingMessage(type, payload);
    }

    public async Task SendInput(Guid targetUserId, string type, object payload)
    {
        await Clients.User(targetUserId.ToString()).ReceiveInputEvent(type, payload);
    }
}
