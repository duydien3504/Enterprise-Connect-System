using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Application.Common.Mappings;
using EnterpriseConnect.MediaService.Application.Common.Models;
using EnterpriseConnect.MediaService.Domain.Entities;
using EnterpriseConnect.MediaService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.InitiateCall;

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

        string? initiatorLiveKitToken = null;
        if (request.Scope == CallScope.Group)
        {
            callSession.LiveKitRoomName = $"room_{callSession.Id:N}";
            initiatorLiveKitToken = _liveKitTokenService.GenerateJoinToken(
                callSession.LiveKitRoomName,
                request.InitiatorId.ToString(),
                "Initiator");
        }

        callSession.Participants.Add(new CallParticipant
        {
            Id = Guid.NewGuid(),
            CallId = callSession.Id,
            UserId = request.InitiatorId,
            Status = ParticipantStatus.Joined,
            JoinTime = DateTimeOffset.UtcNow,
            IsCameraOn = request.CallType == CallType.Video
        });

        var targetParticipants = request.ParticipantIds.Where(id => id != request.InitiatorId).Distinct().ToList();
        foreach (var participantId in targetParticipants)
        {
            callSession.Participants.Add(new CallParticipant
            {
                Id = Guid.NewGuid(),
                CallId = callSession.Id,
                UserId = participantId,
                Status = ParticipantStatus.Inviting
            });
        }

        await _unitOfWork.CallSessions.AddAsync(callSession, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = callSession.ToDto(initiatorLiveKitToken);

        foreach (var recipientId in targetParticipants)
        {
            await _notificationService.NotifyIncomingCallAsync(recipientId, dto);
        }

        return Result<CallSessionDto>.Success(dto);
    }
}
