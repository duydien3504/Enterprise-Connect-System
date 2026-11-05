using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Common.Mappings;
using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.ApproveSession;

public class ApproveSessionCommandHandler : IRequestHandler<ApproveSessionCommand, Result<RemoteSessionDto>>
{
    private readonly IRemoteUnitOfWork _unitOfWork;
    private readonly IRemoteNotificationService _notificationService;

    public ApproveSessionCommandHandler(
        IRemoteUnitOfWork unitOfWork,
        IRemoteNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result<RemoteSessionDto>> Handle(
        ApproveSessionCommand request,
        CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.RemoteSessions.GetByIdWithGrantsAsync(
            request.SessionId, cancellationToken);

        if (session == null)
        {
            return Result<RemoteSessionDto>.Failure("Session not found.");
        }

        if (session.RequesterId != request.RequesterId)
        {
            return Result<RemoteSessionDto>.Failure("You are not authorized to approve this session.");
        }

        if (session.Status != RemoteSessionStatus.PendingApproval)
        {
            return Result<RemoteSessionDto>.Failure($"Session is not pending approval. Current status: {session.Status}.");
        }

        if (request.Accept)
        {
            session.Status = RemoteSessionStatus.Active;
            session.ConnectedAt = DateTimeOffset.UtcNow;

            var viewGrant = session.AccessGrants.FirstOrDefault(g => g.PermissionType == PermissionType.ViewScreen);
            if (viewGrant != null)
            {
                viewGrant.IsGranted = true;
                viewGrant.GrantedAt = DateTimeOffset.UtcNow;
            }

            await _unitOfWork.RemoteSessions.UpdateAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (session.SupporterId.HasValue)
            {
                await _notificationService.NotifyConnectionApprovedAsync(
                    session.SupporterId.Value, session.ToDto());
            }

            return Result<RemoteSessionDto>.Success(session.ToDto());
        }
        else
        {
            session.Status = RemoteSessionStatus.Rejected;
            session.EndedAt = DateTimeOffset.UtcNow;
            session.EndReason = request.RejectionReason ?? "Connection request declined by requester.";

            await _unitOfWork.RemoteSessions.UpdateAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (session.SupporterId.HasValue)
            {
                await _notificationService.NotifyConnectionRejectedAsync(
                    session.SupporterId.Value, session.EndReason);
            }

            return Result<RemoteSessionDto>.Success(session.ToDto());
        }
    }
}
