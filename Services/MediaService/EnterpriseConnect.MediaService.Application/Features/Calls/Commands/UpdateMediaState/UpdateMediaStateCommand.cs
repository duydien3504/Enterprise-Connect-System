using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Application.Common.Mappings;
using EnterpriseConnect.MediaService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.UpdateMediaState;

public record UpdateMediaStateCommand(
    Guid CallId,
    Guid UserId,
    bool? IsMuted = null,
    bool? IsCameraOn = null,
    bool? IsScreenSharing = null) : IRequest<Result<CallParticipantDto>>;

public class UpdateMediaStateCommandHandler : IRequestHandler<UpdateMediaStateCommand, Result<CallParticipantDto>>
{
    private readonly IMediaUnitOfWork _unitOfWork;
    private readonly ICallNotificationService _notificationService;

    public UpdateMediaStateCommandHandler(
        IMediaUnitOfWork unitOfWork,
        ICallNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result<CallParticipantDto>> Handle(UpdateMediaStateCommand request, CancellationToken cancellationToken)
    {
        var call = await _unitOfWork.CallSessions.GetByIdWithParticipantsAsync(request.CallId, cancellationToken);
        if (call == null)
        {
            return Result<CallParticipantDto>.Failure("Call session not found.");
        }

        var participant = call.Participants.FirstOrDefault(p => p.UserId == request.UserId);
        if (participant == null)
        {
            return Result<CallParticipantDto>.Failure("Participant not found in call session.");
        }

        if (request.IsMuted.HasValue) participant.IsMuted = request.IsMuted.Value;
        if (request.IsCameraOn.HasValue) participant.IsCameraOn = request.IsCameraOn.Value;
        if (request.IsScreenSharing.HasValue) participant.IsScreenSharing = request.IsScreenSharing.Value;

        await _unitOfWork.CallSessions.UpdateAsync(call, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyMediaStateChangedAsync(
            call.Id,
            request.UserId,
            request.IsMuted,
            request.IsCameraOn,
            request.IsScreenSharing);

        return Result<CallParticipantDto>.Success(participant.ToDto());
    }
}
