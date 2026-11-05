using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Domain.Enums;

namespace EnterpriseConnect.RemoteService.Application.Common.Interfaces;

public interface IRemoteNotificationService
{
    Task NotifyConnectionRequestedAsync(Guid requesterId, Guid supporterId, string sessionCode);
    Task NotifyConnectionApprovedAsync(Guid supporterId, RemoteSessionDto session);
    Task NotifyConnectionRejectedAsync(Guid supporterId, string reason);
    Task NotifyPermissionRequestedAsync(Guid requesterId, PermissionType permissionType);
    Task NotifyPermissionUpdatedAsync(Guid supporterId, PermissionType permissionType, bool isGranted);
    Task NotifySessionTerminatedAsync(Guid sessionId, string reason);
}
