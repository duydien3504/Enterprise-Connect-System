using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Application.Common.Mappings;
using EnterpriseConnect.MediaService.Application.Common.Models;
using EnterpriseConnect.MediaService.Domain.Entities;
using EnterpriseConnect.MediaService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.InitiateCall;

public record InitiateCallCommand(
    Guid ConversationId,
    Guid InitiatorId,
    CallType CallType,
    CallScope Scope,
    List<Guid> ParticipantIds) : IRequest<Result<CallSessionDto>>;

public class InitiateCallCommandHandler : IRequestHandler<InitiateCallCommand, Result<CallSessionDto>>
{
    private readonly IMediaUnitOfWork _unitOfWork;
    private readonly ILiveKitTokenService _liveKitTokenService;
    private readonly ICallNotificationService _notificationService;

    public InitiateCallCommandHandler(
        IMediaUnitOfWork unitOfWork,
        ILiveKitTokenService liveKitTokenService,
        ICallNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _liveKitTokenService = liveKitTokenService;
        _notificationService = notificationService;
    }

    public async Task<Result<CallSessionDto>> Handle(InitiateCallCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra xem cuộc hội thoại đã có call active chưa
        var activeCall = await _unitOfWork.CallSessions.GetActiveCallByConversationAsync(request.ConversationId, cancellationToken);
        if (activeCall != null)
        {
            return Result<CallSessionDto>.Failure("A call is already in progress for this conversation.");
        }

        var callSession = new CallSession
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            InitiatorId = request.InitiatorId,
            Type = request.CallType,
            Scope = request.Scope,
            Status = CallStatus.Initiated,
            StartTime = DateTimeOffset.UtcNow
        };

        // Nếu là Group Call, khởi tạo LiveKit Room
        string? initiatorLiveKitToken = null;
        if (request.Scope == CallScope.Group)
        {
            callSession.LiveKitRoomName = $"room_{callSession.Id:N}";
            initiatorLiveKitToken = _liveKitTokenService.GenerateJoinToken(
                callSession.LiveKitRoomName,
                request.InitiatorId.ToString(),
                "Initiator");
        }

        // Add Initiator as joined participant
        callSession.Participants.Add(new CallParticipant
        {
            Id = Guid.NewGuid(),
            CallId = callSession.Id,
            UserId = request.InitiatorId,
            Status = ParticipantStatus.Joined,
            JoinTime = DateTimeOffset.UtcNow
        });

        // Add Invited Participants
        var targetParticipants = request.ParticipantIds.Distinct().Where(id => id != request.InitiatorId).ToList();
        foreach (var pId in targetParticipants)
        {
            callSession.Participants.Add(new CallParticipant
            {
                Id = Guid.NewGuid(),
                CallId = callSession.Id,
                UserId = pId,
                Status = ParticipantStatus.Inviting
            });
        }

        await _unitOfWork.CallSessions.AddAsync(callSession, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = callSession.ToDto(initiatorLiveKitToken);

        // Phát tín hiệu Incoming Call tới tất cả người được mời qua SignalR
        foreach (var pId in targetParticipants)
        {
            await _notificationService.NotifyIncomingCallAsync(pId, dto);
        }

        return Result<CallSessionDto>.Success(dto);
    }
}
