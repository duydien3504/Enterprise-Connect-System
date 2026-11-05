using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Common.Mappings;
using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.TerminateSession;

public class TerminateSessionCommandHandler : IRequestHandler<TerminateSessionCommand, Result<RemoteSessionDto>>
{
    private readonly IRemoteUnitOfWork _unitOfWork;
    private readonly IRemoteNotificationService _notificationService;

    public TerminateSessionCommandHandler(
        IRemoteUnitOfWork unitOfWork,
        IRemoteNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result<RemoteSessionDto>> Handle(
        TerminateSessionCommand request,
        CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.RemoteSessions.GetByIdWithGrantsAsync(
            request.SessionId, cancellationToken);

        if (session == null)
        {
            return Result<RemoteSessionDto>.Failure("Session not found.");
        }

        if (session.RequesterId != request.UserId && session.SupporterId != request.UserId)
        {
            return Result<RemoteSessionDto>.Failure("You are not a participant in this session.");
        }

        if (session.Status == RemoteSessionStatus.Terminated || session.Status == RemoteSessionStatus.Expired)
        {
            return Result<RemoteSessionDto>.Success(session.ToDto());
        }

        session.Status = RemoteSessionStatus.Terminated;
        session.EndedAt = DateTimeOffset.UtcNow;
        session.EndReason = request.Reason ?? "Session terminated by user.";

        if (session.ConnectedAt.HasValue)
        {
            session.DurationSeconds = (int)(session.EndedAt.Value - session.ConnectedAt.Value).TotalSeconds;
        }

        foreach (var grant in session.AccessGrants)
        {
            grant.IsGranted = false;
            grant.RevokedAt = DateTimeOffset.UtcNow;
        }

        await _unitOfWork.RemoteSessions.UpdateAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifySessionTerminatedAsync(session.Id, session.EndReason);

        return Result<RemoteSessionDto>.Success(session.ToDto());
    }
}
