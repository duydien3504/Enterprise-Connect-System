using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Mappings;
using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Moderation.Queries.GetViolationById;

public class GetViolationByIdQueryHandler : IRequestHandler<GetViolationByIdQuery, Result<ToxicViolationDto>>
{
    private readonly IAdminUnitOfWork _unitOfWork;

    public GetViolationByIdQueryHandler(IAdminUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ToxicViolationDto>> Handle(GetViolationByIdQuery request, CancellationToken cancellationToken)
    {
        var violation = await _unitOfWork.ToxicViolations.GetByIdAsync(request.Id, cancellationToken);
        if (violation == null)
        {
            return Result<ToxicViolationDto>.Failure($"Violation with Id '{request.Id}' was not found.");
        }

        return Result<ToxicViolationDto>.Success(violation.ToDto());
    }
}
