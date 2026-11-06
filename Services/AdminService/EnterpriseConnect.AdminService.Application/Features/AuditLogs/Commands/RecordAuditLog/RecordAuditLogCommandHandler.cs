using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Mappings;
using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Entities;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.AuditLogs.Commands.RecordAuditLog;

public class RecordAuditLogCommandHandler : IRequestHandler<RecordAuditLogCommand, Result<AuditLogDto>>
{
    private readonly IAdminUnitOfWork _unitOfWork;
    private readonly IAuditLogDeduplicator _deduplicator;

    public RecordAuditLogCommandHandler(
        IAdminUnitOfWork _uow,
        IAuditLogDeduplicator deduplicator)
    {
        _unitOfWork = _uow;
        _deduplicator = deduplicator;
    }

    public async Task<Result<AuditLogDto>> Handle(RecordAuditLogCommand request, CancellationToken cancellationToken)
    {
        if (_deduplicator.IsDuplicate(
            request.CorrelationId,
            request.ServiceName,
            request.ActionName,
            request.ActorId,
            request.EndpointUrl))
        {
            return Result<AuditLogDto>.Failure("Duplicate audit log detected and suppressed.");
        }

        var log = new AuditLog
        {
            CorrelationId = request.CorrelationId,
            Timestamp = DateTimeOffset.UtcNow,
            ServiceName = request.ServiceName,
            ActionName = request.ActionName,
            TargetEntity = request.TargetEntity,
            TargetId = request.TargetId,
            ActorId = request.ActorId,
            ActorEmail = request.ActorEmail,
            ActorRole = request.ActorRole,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            HttpMethod = request.HttpMethod,
            EndpointUrl = request.EndpointUrl,
            StatusCode = request.StatusCode,
            ExecutionDurationMs = request.ExecutionDurationMs,
            RiskLevel = request.RiskLevel,
            Details = request.Details,
            FailureReason = request.FailureReason
        };

        await _unitOfWork.AuditLogs.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuditLogDto>.Success(log.ToDto());
    }
}
