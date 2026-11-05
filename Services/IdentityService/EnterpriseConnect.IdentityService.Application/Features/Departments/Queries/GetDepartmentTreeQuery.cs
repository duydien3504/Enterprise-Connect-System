using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Departments.Queries;

public record GetDepartmentTreeQuery : IRequest<IReadOnlyList<DepartmentTreeDto>>;
