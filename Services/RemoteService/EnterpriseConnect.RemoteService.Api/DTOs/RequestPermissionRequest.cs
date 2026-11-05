using EnterpriseConnect.RemoteService.Domain.Enums;

namespace EnterpriseConnect.RemoteService.Api.DTOs;

public record RequestPermissionRequest(
    PermissionType PermissionType
);
