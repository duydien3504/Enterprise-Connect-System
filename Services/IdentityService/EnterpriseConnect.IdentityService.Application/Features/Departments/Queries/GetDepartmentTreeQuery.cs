using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Departments.Queries;

public record GetDepartmentTreeQuery : IRequest<IReadOnlyList<DepartmentTreeDto>>;

public class GetDepartmentTreeQueryHandler : IRequestHandler<GetDepartmentTreeQuery, IReadOnlyList<DepartmentTreeDto>>
{
    private readonly IIdentityUnitOfWork _uow;

    public GetDepartmentTreeQueryHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IReadOnlyList<DepartmentTreeDto>> Handle(GetDepartmentTreeQuery request, CancellationToken cancellationToken)
    {
        return await _uow.Departments.GetDepartmentTreeAsync(cancellationToken);
    }
}
