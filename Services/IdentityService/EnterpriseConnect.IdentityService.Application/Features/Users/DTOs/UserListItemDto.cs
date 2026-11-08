namespace EnterpriseConnect.IdentityService.Application.Features.Users.DTOs;

public record UserListItemDto(
    Guid Id,
    string FullName,
    string? DepartmentName,
    Guid? DepartmentId,
    string Email,
    string? Position,
    string? AvatarUrl,
    string? Phone
);

public record GetUsersResultDto(
    IReadOnlyList<UserListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
