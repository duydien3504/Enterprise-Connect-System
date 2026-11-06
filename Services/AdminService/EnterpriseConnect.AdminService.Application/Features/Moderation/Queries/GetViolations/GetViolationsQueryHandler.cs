using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Mappings;
using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Moderation.Queries.GetViolations;

public class GetViolationsQueryHandler 
    : IRequestHandler<GetViolationsQuery, Result<(List<ToxicViolationDto> Items, long TotalCount)>>
{
    private readonly IAdminUnitOfWork _unitOfWork;

    public GetViolationsQueryHandler(IAdminUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<(List<ToxicViolationDto> Items, long TotalCount)>> Handle(
        GetViolationsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, total) = await _unitOfWork.ToxicViolations.GetPagedAsync(
            request.ViolationType,
            request.Severity,
            request.ActionTaken,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = items.Select(x => x.ToDto()).ToList();
        return Result<(List<ToxicViolationDto> Items, long TotalCount)>.Success((dtos, total));
    }
}
