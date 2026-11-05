using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Users.DTOs;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.Queries;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, SearchUsersResultDto>
{
    private readonly IIdentityUnitOfWork _uow;

    public SearchUsersQueryHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<SearchUsersResultDto> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var (items, totalCount) = await _uow.Users.SearchUsersAsync(request.Keyword, request.DepartmentId, page, pageSize, cancellationToken);
        return new SearchUsersResultDto(items, totalCount, page, pageSize);
    }
}
