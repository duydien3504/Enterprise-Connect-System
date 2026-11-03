using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Application.Common.Mappings;
using EnterpriseConnect.MediaService.Application.Common.Models;
using EnterpriseConnect.MediaService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.RespondCall;

public record RespondCallCommand(
    Guid CallId,
    Guid UserId,
    bool Accept,
    string? DeclineReason = null) : IRequest<Result<CallSessionDto>>;

public class RespondCallCommandHandler : IRequestHandler<RespondCallCommand, Result<CallSessionDto>>
{
    private readonly IMediaUnitOfWork _unitOfWork;
    private readonly ILiveKitTokenService _liveKitTokenService;
    private readonly ICallNotificationService _notificationService;

    public RespondCallCommandHandler(
        IMediaUnitOfWork unitOfWork,
        ILiveKitTokenService liveKitTokenService,
        ICallNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _liveKitTokenService = liveKitTokenService;
        _notificationService = notificationService;
    }

    public async Task<Result<CallSessionDto>> Handle(RespondCallCommand request, CancellationToken cancellationToken)
    {
        var call = await _unitOfWork.CallSessions.GetByIdWithParticipantsAsync(request.CallId, cancellationToken);
        if (call == null)
        {
            return Result<CallSessionDto>.Failure("Call session not found.");
        }

        if (call.Status == CallStatus.Ended || call.Status == CallStatus.Declined || call.Status == CallStatus.Missed)
        {
            return Result<CallSessionDto>.Failure($"Cannot respond. Call has already ended with status '{call.Status}'.");
        }

        var participant = call.Participants.FirstOrDefault(p => p.UserId == request.UserId);
        if (participant == null)
        {
            return Result<CallSessionDto>.Failure("User is not invited to this call session.");
        }

        string? liveKitToken = null;

        if (request.Accept)
        {
            participant.Status = ParticipantStatus.Joined;
            participant.JoinTime = DateTimeOffset.UtcNow;

            if (call.Status == CallStatus.Initiated || call.Status == CallStatus.Ringing)
            {
                call.Status = CallStatus.Ongoing;
                call.ConnectedAt = DateTimeOffset.UtcNow;
            }

            if (call.Scope == CallScope.Group && !string.IsNullOrEmpty(call.LiveKitRoomName))
            {
                liveKitToken = _liveKitTokenService.GenerateJoinToken(
                    call.LiveKitRoomName,
                    request.UserId.ToString(),
                    "Participant");
            }

            await _unitOfWork.CallSessions.UpdateAsync(call, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = call.ToDto(liveKitToken);
            await _notificationService.NotifyCallAnsweredAsync(call.Id, request.UserId, dto);
            return Result<CallSessionDto>.Success(dto);
        }
        else
        {
            // Từ chối cuộc gọi
            participant.Status = ParticipantStatus.Declined;
            participant.LeaveTime = DateTimeOffset.UtcNow;

            // Nếu là cuộc gọi 1-1 mà người nhận từ chối -> Cuộc gọi kết thúc luôn
            if (call.Scope == CallScope.Direct)
            {
                call.Status = CallStatus.Declined;
                call.EndTime = DateTimeOffset.UtcNow;
                call.EndReason = request.DeclineReason ?? "Declined by recipient";
            }
            else
            {
                // Group Call: Chỉ kết thúc nếu không còn ai ngoài initiator
                var hasOtherActive = call.Participants.Any(p => p.UserId != call.InitiatorId && (p.Status == ParticipantStatus.Joined || p.Status == ParticipantStatus.Inviting));
                if (!hasOtherActive)
                {
                    call.Status = CallStatus.Declined;
                    call.EndTime = DateTimeOffset.UtcNow;
                    call.EndReason = "All invitees declined.";
                }
            }

            await _unitOfWork.CallSessions.UpdateAsync(call, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = call.ToDto();
            await _notificationService.NotifyCallDeclinedAsync(call.Id, request.UserId, request.DeclineReason ?? "User declined");
            return Result<CallSessionDto>.Success(dto);
        }
    }
}
