using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Users.DTOs;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.Queries;

public class GetUsersListQueryHandler : IRequestHandler<GetUsersListQuery, GetUsersResultDto>
{
    private readonly IIdentityUnitOfWork _uow;

    public GetUsersListQueryHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<GetUsersResultDto> Handle(GetUsersListQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var (items, totalCount) = await _uow.Users.GetUsersListAsync(
            request.Name,
            request.DepartmentId,
            request.DepartmentName,
            page,
            pageSize,
            cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new GetUsersResultDto(items, totalCount, page, pageSize, totalPages);
    }
}
