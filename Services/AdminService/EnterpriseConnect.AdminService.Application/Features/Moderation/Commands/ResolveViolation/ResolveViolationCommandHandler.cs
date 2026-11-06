using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Mappings;
using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Moderation.Commands.ResolveViolation;

public class ResolveViolationCommandHandler : IRequestHandler<ResolveViolationCommand, Result<ToxicViolationDto>>
{
    private readonly IAdminUnitOfWork _unitOfWork;

    public ResolveViolationCommandHandler(IAdminUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ToxicViolationDto>> Handle(ResolveViolationCommand request, CancellationToken cancellationToken)
    {
        var violation = await _unitOfWork.ToxicViolations.GetByIdAsync(request.Id, cancellationToken);
        if (violation == null)
        {
            return Result<ToxicViolationDto>.Failure($"Violation with Id '{request.Id}' was not found.");
        }

        violation.ActionTaken = request.ActionTaken;
        violation.UpdatedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.ToxicViolations.UpdateAsync(violation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ToxicViolationDto>.Success(violation.ToDto());
    }
}
