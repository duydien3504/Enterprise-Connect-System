using EnterpriseConnect.MediaService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.RespondCall;

public record RespondCallCommand(
    Guid CallId,
    Guid UserId,
    bool Accept,
    string? DeclineReason = null
) : IRequest<Result<CallSessionDto>>;
