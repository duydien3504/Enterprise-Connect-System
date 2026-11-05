using EnterpriseConnect.RemoteService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.CreateSession;

public record CreateSessionCommand(
    Guid RequesterId,
    int ExpirationMinutes = 15
) : IRequest<Result<RemoteSessionDto>>;
