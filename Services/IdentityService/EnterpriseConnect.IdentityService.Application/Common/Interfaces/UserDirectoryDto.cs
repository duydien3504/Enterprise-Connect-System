namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public record UserDirectoryDto(
    Guid Id,
    string FullName,
    string Email,
    string? Phone,
    string? Position,
    string? AvatarUrl,
    Guid? DepartmentId,
    string? DepartmentName
);
