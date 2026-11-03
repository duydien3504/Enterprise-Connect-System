using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Application.Common.Mappings;
using EnterpriseConnect.MediaService.Application.Common.Models;
using EnterpriseConnect.MediaService.Domain.Entities;
using EnterpriseConnect.MediaService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.MediaService.Application.Features.Calls.Commands.InviteParticipant;

public record InviteParticipantCommand(
    Guid CallId,
    Guid InviterId,
    List<Guid> UserIds) : IRequest<Result<CallSessionDto>>;

public class InviteParticipantCommandHandler : IRequestHandler<InviteParticipantCommand, Result<CallSessionDto>>
{
    private readonly IMediaUnitOfWork _unitOfWork;
    private readonly ICallNotificationService _notificationService;

    public InviteParticipantCommandHandler(
        IMediaUnitOfWork unitOfWork,
        ICallNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result<CallSessionDto>> Handle(InviteParticipantCommand request, CancellationToken cancellationToken)
    {
        var call = await _unitOfWork.CallSessions.GetByIdWithParticipantsAsync(request.CallId, cancellationToken);
        if (call == null)
        {
            return Result<CallSessionDto>.Failure("Call session not found.");
        }

        if (call.Status != CallStatus.Ongoing && call.Status != CallStatus.Initiated && call.Status != CallStatus.Ringing)
        {
            return Result<CallSessionDto>.Failure("Cannot invite participants to an inactive call.");
        }

        var newParticipants = new List<CallParticipant>();
        foreach (var userId in request.UserIds.Distinct())
        {
            var existing = call.Participants.FirstOrDefault(p => p.UserId == userId);
            if (existing == null)
            {
                var p = new CallParticipant
                {
                    Id = Guid.NewGuid(),
                    CallId = call.Id,
                    UserId = userId,
                    Status = ParticipantStatus.Inviting
                };
                call.Participants.Add(p);
                newParticipants.Add(p);
            }
            else if (existing.Status == ParticipantStatus.Left || existing.Status == ParticipantStatus.Declined)
            {
                existing.Status = ParticipantStatus.Inviting;
                existing.LeaveTime = null;
                newParticipants.Add(existing);
            }
        }

        await _unitOfWork.CallSessions.UpdateAsync(call, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = call.ToDto();
        foreach (var p in newParticipants)
        {
            await _notificationService.NotifyIncomingCallAsync(p.UserId, dto);
        }

        return Result<CallSessionDto>.Success(dto);
    }
}
