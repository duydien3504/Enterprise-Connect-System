using EnterpriseConnect.MediaService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.UpdateMediaState;

public record UpdateMediaStateCommand(
    Guid CallId,
    Guid UserId,
    bool? IsMuted = null,
    bool? IsCameraOn = null,
    bool? IsScreenSharing = null
) : IRequest<Result<CallParticipantDto>>;
