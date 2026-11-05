using EnterpriseConnect.IdentityService.Application.Features.Users.DTOs;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.Queries;

public record SearchUsersQuery(
    string? Keyword,
    Guid? DepartmentId,
    int Page = 1,
    int PageSize = 20
) : IRequest<SearchUsersResultDto>;
