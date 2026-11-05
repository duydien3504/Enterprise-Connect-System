using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Domain.Enums;

namespace EnterpriseConnect.RemoteService.Api.Hubs;

public interface IRemoteClient
{
    Task ReceiveConnectionRequest(Guid requesterId, Guid supporterId, string sessionCode);
    Task ReceiveConnectionApproval(RemoteSessionDto session);
    Task ReceiveConnectionRejection(string reason);
    Task ReceivePermissionRequest(PermissionType permissionType);
    Task ReceivePermissionUpdate(PermissionType permissionType, bool isGranted);
    Task ReceiveSessionTermination(string reason);
    Task ReceiveSignalingMessage(string type, object payload);
    Task ReceiveInputEvent(string type, object payload);
}
