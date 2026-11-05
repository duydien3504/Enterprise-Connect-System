namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public record UserRolePermissionsDto(IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);
