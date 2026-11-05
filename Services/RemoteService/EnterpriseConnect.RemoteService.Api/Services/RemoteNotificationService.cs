using EnterpriseConnect.RemoteService.Api.Hubs;
using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace EnterpriseConnect.RemoteService.Api.Services;

public class RemoteNotificationService : IRemoteNotificationService
{
    private readonly IHubContext<RemoteHub, IRemoteClient> _hubContext;

    public RemoteNotificationService(IHubContext<RemoteHub, IRemoteClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyConnectionRequestedAsync(Guid requesterId, Guid supporterId, string sessionCode)
    {
        await _hubContext.Clients.User(requesterId.ToString())
            .ReceiveConnectionRequest(requesterId, supporterId, sessionCode);
    }

    public async Task NotifyConnectionApprovedAsync(Guid supporterId, RemoteSessionDto session)
    {
        await _hubContext.Clients.User(supporterId.ToString())
            .ReceiveConnectionApproval(session);
    }

    public async Task NotifyConnectionRejectedAsync(Guid supporterId, string reason)
    {
        await _hubContext.Clients.User(supporterId.ToString())
            .ReceiveConnectionRejection(reason);
    }

    public async Task NotifyPermissionRequestedAsync(Guid requesterId, PermissionType permissionType)
    {
        await _hubContext.Clients.User(requesterId.ToString())
            .ReceivePermissionRequest(permissionType);
    }

    public async Task NotifyPermissionUpdatedAsync(Guid supporterId, PermissionType permissionType, bool isGranted)
    {
        await _hubContext.Clients.User(supporterId.ToString())
            .ReceivePermissionUpdate(permissionType, isGranted);
    }

    public async Task NotifySessionTerminatedAsync(Guid sessionId, string reason)
    {
        await _hubContext.Clients.Group(sessionId.ToString())
            .ReceiveSessionTermination(reason);
    }
}
