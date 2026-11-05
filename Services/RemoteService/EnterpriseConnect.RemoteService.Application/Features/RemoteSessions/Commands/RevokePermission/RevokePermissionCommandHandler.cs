using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Common.Mappings;
using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RevokePermission;

public class RevokePermissionCommandHandler : IRequestHandler<RevokePermissionCommand, Result<AccessGrantDto>>
{
    private readonly IRemoteUnitOfWork _unitOfWork;
    private readonly IRemoteNotificationService _notificationService;

    public RevokePermissionCommandHandler(
        IRemoteUnitOfWork unitOfWork,
        IRemoteNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result<AccessGrantDto>> Handle(
        RevokePermissionCommand request,
        CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.RemoteSessions.GetByIdWithGrantsAsync(
            request.SessionId, cancellationToken);

        if (session == null)
        {
            return Result<AccessGrantDto>.Failure("Session not found.");
        }

        if (session.Status != RemoteSessionStatus.Active)
        {
            return Result<AccessGrantDto>.Failure("Cannot revoke permissions on an inactive session.");
        }

        if (session.RequesterId != request.RequesterId)
        {
            return Result<AccessGrantDto>.Failure("Only the session owner can revoke permissions.");
        }

        var grant = session.AccessGrants.FirstOrDefault(g => g.PermissionType == request.PermissionType);
        if (grant == null)
        {
            return Result<AccessGrantDto>.Failure($"Grant for {request.PermissionType} does not exist.");
        }

        grant.IsGranted = false;
        grant.RevokedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.RemoteSessions.UpdateAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (session.SupporterId.HasValue)
        {
            await _notificationService.NotifyPermissionUpdatedAsync(
                session.SupporterId.Value, request.PermissionType, false);
        }

        return Result<AccessGrantDto>.Success(grant.ToDto());
    }
}
