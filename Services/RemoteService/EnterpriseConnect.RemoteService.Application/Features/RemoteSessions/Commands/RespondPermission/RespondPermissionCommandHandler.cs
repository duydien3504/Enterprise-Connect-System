using EnterpriseConnect.BuildingBlocks.Application.Audit;
using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Common.Mappings;
using EnterpriseConnect.RemoteService.Application.Common.Models;
using EnterpriseConnect.RemoteService.Domain.Entities;
using EnterpriseConnect.RemoteService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RespondPermission;

public class RespondPermissionCommandHandler : IRequestHandler<RespondPermissionCommand, Result<AccessGrantDto>>
{
    private readonly IRemoteUnitOfWork _unitOfWork;
    private readonly IRemoteNotificationService _notificationService;
    private readonly IAuditClient? _auditClient;

    public RespondPermissionCommandHandler(
        IRemoteUnitOfWork unitOfWork,
        IRemoteNotificationService notificationService,
        IAuditClient? auditClient = null)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _auditClient = auditClient;
    }

    public async Task<Result<AccessGrantDto>> Handle(RespondPermissionCommand request, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.RemoteSessions.GetByIdWithGrantsAsync(request.SessionId, cancellationToken);
        if (session == null) return Result<AccessGrantDto>.Failure("Session not found.");
        if (session.Status != RemoteSessionStatus.Active) return Result<AccessGrantDto>.Failure("Cannot update permissions on an inactive session.");
        if (session.RequesterId != request.RequesterId) return Result<AccessGrantDto>.Failure("Only the session owner can grant or decline permissions.");

        var grant = session.AccessGrants.FirstOrDefault(g => g.PermissionType == request.PermissionType);
        if (grant == null)
        {
            grant = new AccessGrant { Id = Guid.NewGuid(), SessionId = session.Id, PermissionType = request.PermissionType };
            session.AccessGrants.Add(grant);
        }

        grant.IsGranted = request.Grant;
        grant.GrantedAt = request.Grant ? DateTimeOffset.UtcNow : grant.GrantedAt;
        grant.RevokedAt = request.Grant ? null : DateTimeOffset.UtcNow;

        await _unitOfWork.RemoteSessions.UpdateAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (session.SupporterId.HasValue)
        {
            await _notificationService.NotifyPermissionUpdatedAsync(session.SupporterId.Value, request.PermissionType, request.Grant);
        }

        _ = _auditClient?.RecordEventAsync(new AuditEventDto
        {
            ServiceName = "RemoteService",
            ActionName = request.Grant ? "REMOTE_PERMISSION_GRANTED" : "REMOTE_PERMISSION_REVOKED",
            TargetEntity = "RemoteSession",
            TargetId = session.Id.ToString(),
            ActorId = request.RequesterId,
            RiskLevel = request.Grant ? (request.PermissionType == PermissionType.ControlInput ? 3 : 2) : 1,
            Details = request.Grant 
                ? $"Session owner granted permission '{request.PermissionType}' to supporter '{session.SupporterId}'."
                : $"Session owner revoked permission '{request.PermissionType}'."
        }, cancellationToken);

        return Result<AccessGrantDto>.Success(grant.ToDto());
    }
}
