using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RespondPermission;

public record RespondPermissionCommand(
    Guid SessionId,
    Guid RequesterId,
    PermissionType PermissionType,
    bool Grant
) : IRequest<Result<AccessGrantDto>>;
