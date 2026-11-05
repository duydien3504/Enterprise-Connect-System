namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public record ContactItemDto(
    Guid ContactId,
    Guid UserId,
    string FullName,
    string Email,
    string? AvatarUrl,
    string? Position,
    string? DepartmentName,
    int Status,
    bool IsRequester,
    DateTimeOffset CreatedAt
);
