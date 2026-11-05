using EnterpriseConnect.IdentityService.Domain.Enums;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.DTOs;

public record UserProfileDto(
    Guid Id,
    string FullName,
    string Email,
    string? Phone,
    string? Position,
    string? AvatarUrl,
    Guid? DepartmentId,
    string? DepartmentName,
    UserStatus Status,
    bool EmailConfirmed,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions
);
