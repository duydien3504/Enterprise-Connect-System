using EnterpriseConnect.MediaService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.InviteParticipant;

public record InviteParticipantCommand(
    Guid CallId,
    Guid InviterId,
    List<Guid> UserIds
) : IRequest<Result<CallSessionDto>>;
