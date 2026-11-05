namespace EnterpriseConnect.IdentityService.Application.Features.Roles.Commands;

public record RoleDto(Guid Id, string Name, string? Description, IReadOnlyList<string> Permissions);
