using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Application.Common.Mappings;
using EnterpriseConnect.MediaService.Application.Common.Models;
using EnterpriseConnect.MediaService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.EndCall;

public record EndCallCommand(
    Guid CallId,
    Guid UserId,
    string? Reason = null) : IRequest<Result<CallSessionDto>>;

public class EndCallCommandHandler : IRequestHandler<EndCallCommand, Result<CallSessionDto>>
{
    private readonly IMediaUnitOfWork _unitOfWork;
    private readonly ICallNotificationService _notificationService;

    public EndCallCommandHandler(
        IMediaUnitOfWork unitOfWork,
        ICallNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result<CallSessionDto>> Handle(EndCallCommand request, CancellationToken cancellationToken)
    {
        var call = await _unitOfWork.CallSessions.GetByIdWithParticipantsAsync(request.CallId, cancellationToken);
        if (call == null)
        {
            return Result<CallSessionDto>.Failure("Call session not found.");
        }

        if (call.Status == CallStatus.Ended)
        {
            return Result<CallSessionDto>.Success(call.ToDto());
        }

        var participant = call.Participants.FirstOrDefault(p => p.UserId == request.UserId);
        if (participant == null)
        {
            return Result<CallSessionDto>.Failure("User is not a participant of this call.");
        }

        // Cập nhật trạng thái người rời cuộc gọi
        participant.Status = ParticipantStatus.Left;
        participant.LeaveTime = DateTimeOffset.UtcNow;

        // Nếu là Direct 1-1, hoặc người kết thúc là Initiator, hoặc không còn người tham gia active -> Kết thúc toàn bộ cuộc gọi
        bool shouldEndWholeCall = call.Scope == CallScope.Direct 
            || call.InitiatorId == request.UserId 
            || !call.Participants.Any(p => p.Status == ParticipantStatus.Joined && p.UserId != request.UserId);

        if (shouldEndWholeCall)
        {
            call.Status = CallStatus.Ended;
            call.EndTime = DateTimeOffset.UtcNow;
            call.EndReason = request.Reason ?? "Call ended by user.";

            if (call.ConnectedAt.HasValue)
            {
                call.DurationSeconds = (int)(call.EndTime.Value - call.ConnectedAt.Value).TotalSeconds;
            }

            foreach (var p in call.Participants.Where(p => p.Status == ParticipantStatus.Joined || p.Status == ParticipantStatus.Inviting))
            {
                p.Status = ParticipantStatus.Left;
                p.LeaveTime ??= call.EndTime;
            }

            await _unitOfWork.CallSessions.UpdateAsync(call, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = call.ToDto();
            await _notificationService.NotifyCallEndedAsync(call.Id, call.EndReason, call.DurationSeconds);
            return Result<CallSessionDto>.Success(dto);
        }
        else
        {
            // Chỉ rời phòng (Group Call)
            await _unitOfWork.CallSessions.UpdateAsync(call, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = call.ToDto();
            await _notificationService.NotifyParticipantLeftAsync(call.Id, request.UserId);
            return Result<CallSessionDto>.Success(dto);
        }
    }
}
