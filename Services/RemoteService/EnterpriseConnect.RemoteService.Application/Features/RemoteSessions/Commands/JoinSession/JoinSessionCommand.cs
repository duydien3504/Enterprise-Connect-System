using EnterpriseConnect.RemoteService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.JoinSession;

public record JoinSessionCommand(
    string SessionCode,
    Guid SupporterId
) : IRequest<Result<RemoteSessionDto>>;
