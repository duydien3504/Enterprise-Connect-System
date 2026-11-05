using EnterpriseConnect.IdentityService.Application.Common.Interfaces;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.DTOs;

public record SearchUsersResultDto(
    IReadOnlyList<UserDirectoryDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);
