using EnterpriseConnect.MediaService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.EndCall;

public record EndCallCommand(
    Guid CallId,
    Guid UserId,
    string? Reason = null
) : IRequest<Result<CallSessionDto>>;
