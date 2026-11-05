using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Common.Mappings;
using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.JoinSession;

public class JoinSessionCommandHandler : IRequestHandler<JoinSessionCommand, Result<RemoteSessionDto>>
{
    private readonly IRemoteUnitOfWork _unitOfWork;
    private readonly IRemoteNotificationService _notificationService;

    public JoinSessionCommandHandler(
        IRemoteUnitOfWork unitOfWork,
        IRemoteNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result<RemoteSessionDto>> Handle(
        JoinSessionCommand request,
        CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.RemoteSessions.GetByCodeWithGrantsAsync(
            request.SessionCode.Trim(), cancellationToken);

        if (session == null)
        {
            return Result<RemoteSessionDto>.Failure("Invalid or non-existent session code.");
        }

        if (session.ExpiresAt.HasValue && session.ExpiresAt.Value < DateTimeOffset.UtcNow)
        {
            session.Status = RemoteSessionStatus.Expired;
            await _unitOfWork.RemoteSessions.UpdateAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<RemoteSessionDto>.Failure("This session code has expired.");
        }

        if (session.Status != RemoteSessionStatus.Created)
        {
            return Result<RemoteSessionDto>.Failure($"Session is not available for joining. Current status: {session.Status}.");
        }

        if (session.RequesterId == request.SupporterId)
        {
            return Result<RemoteSessionDto>.Failure("You cannot join your own remote assistance session as a supporter.");
        }

        session.SupporterId = request.SupporterId;
        session.Status = RemoteSessionStatus.PendingApproval;

        await _unitOfWork.RemoteSessions.UpdateAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyConnectionRequestedAsync(
            session.RequesterId, request.SupporterId, session.SessionCode);

        return Result<RemoteSessionDto>.Success(session.ToDto());
    }
}
