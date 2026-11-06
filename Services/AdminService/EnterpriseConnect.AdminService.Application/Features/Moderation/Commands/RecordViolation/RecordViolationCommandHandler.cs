using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Mappings;
using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Entities;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Moderation.Commands.RecordViolation;

public class RecordViolationCommandHandler : IRequestHandler<RecordViolationCommand, Result<ToxicViolationDto>>
{
    private readonly IAdminUnitOfWork _unitOfWork;

    public RecordViolationCommandHandler(IAdminUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ToxicViolationDto>> Handle(RecordViolationCommand request, CancellationToken cancellationToken)
    {
        var violation = new ToxicViolation
        {
            MessageId = request.MessageId,
            SenderId = request.SenderId,
            ViolationType = request.ViolationType,
            Severity = request.Severity,
            ActionTaken = request.ActionTaken,
            DetectedAt = DateTimeOffset.UtcNow
        };

        await _unitOfWork.ToxicViolations.AddAsync(violation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ToxicViolationDto>.Success(violation.ToDto());
    }
}
