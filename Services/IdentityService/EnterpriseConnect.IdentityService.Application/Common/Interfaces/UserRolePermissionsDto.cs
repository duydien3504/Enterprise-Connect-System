namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public record UserRolePermissionsDto(IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions)
{
    public IReadOnlyList<Guid> RoleIds { get; init; } = Array.Empty<Guid>();
}

