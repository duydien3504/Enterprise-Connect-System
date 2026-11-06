using EnterpriseConnect.BuildingBlocks.Application.Audit;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.Commands;

public class LockUserCommandHandler : IRequestHandler<LockUserCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;
    private readonly IAuditClient? _auditClient;

    public LockUserCommandHandler(IIdentityUnitOfWork uow, IAuditClient? auditClient = null)
    {
        _uow = uow;
        _auditClient = auditClient;
    }

    public async Task<bool> Handle(LockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        user.Status = UserStatus.Inactive;
        user.LockReason = request.Reason;
        user.LockedAt = DateTimeOffset.UtcNow;

        await _uow.Users.UpdateAsync(user, cancellationToken);
        await _uow.UserSessions.RevokeAllSessionsByUserIdAsync(user.Id, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        _ = _auditClient?.RecordEventAsync(new AuditEventDto
        {
            ServiceName = "IdentityService",
            ActionName = "USER_LOCKED",
            TargetEntity = "User",
            TargetId = user.Id.ToString(),
            ActorId = user.Id,
            ActorEmail = user.Email,
            RiskLevel = 3,
            Details = $"User account locked. Reason: {request.Reason}"
        }, cancellationToken);

        return true;
    }
}
