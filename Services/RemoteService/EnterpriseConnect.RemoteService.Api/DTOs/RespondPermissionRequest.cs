using EnterpriseConnect.RemoteService.Domain.Enums;

namespace EnterpriseConnect.RemoteService.Api.DTOs;

public record RespondPermissionRequest(
    PermissionType PermissionType,
    bool Grant
);
