using EnterpriseConnect.MediaService.Application.Common.Models;
using EnterpriseConnect.MediaService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.InitiateCall;

public record InitiateCallCommand(
    Guid ConversationId,
    Guid InitiatorId,
    CallType CallType,
    CallScope Scope,
    List<Guid> ParticipantIds
) : IRequest<Result<CallSessionDto>>;
