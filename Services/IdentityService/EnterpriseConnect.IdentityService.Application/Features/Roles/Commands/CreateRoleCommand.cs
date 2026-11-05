using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Roles.Commands;

public record CreateRoleCommand(string Name, string? Description, List<string>? PermissionNames) : IRequest<Guid>;
