using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Roles.Commands;

public record GetAllRolesQuery : IRequest<IReadOnlyList<RoleDto>>;
