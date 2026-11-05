using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RevokePermission;

public record RevokePermissionCommand(
    Guid SessionId,
    Guid RequesterId,
    PermissionType PermissionType
) : IRequest<Result<AccessGrantDto>>;
