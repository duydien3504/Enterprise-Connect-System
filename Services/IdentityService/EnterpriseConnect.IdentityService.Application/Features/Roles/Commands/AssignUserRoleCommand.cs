using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Roles.Commands;

public record AssignUserRoleCommand(Guid UserId, Guid RoleId) : IRequest<bool>;
