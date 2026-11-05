using EnterpriseConnect.RemoteService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.TerminateSession;

public record TerminateSessionCommand(
    Guid SessionId,
    Guid UserId,
    string? Reason = null
) : IRequest<Result<RemoteSessionDto>>;
