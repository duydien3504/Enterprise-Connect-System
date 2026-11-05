using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RequestPermission;

public record RequestPermissionCommand(
    Guid SessionId,
    Guid SupporterId,
    PermissionType PermissionType
) : IRequest<Result<bool>>;
