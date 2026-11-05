using EnterpriseConnect.RemoteService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.ApproveSession;

public record ApproveSessionCommand(
    Guid SessionId,
    Guid RequesterId,
    bool Accept,
    string? RejectionReason = null
) : IRequest<Result<RemoteSessionDto>>;
