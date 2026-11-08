using EnterpriseConnect.IdentityService.Application.Features.Users.DTOs;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.Queries;

public record GetUsersListQuery(
    string? Name = null,
    Guid? DepartmentId = null,
    string? DepartmentName = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<GetUsersResultDto>;
