namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public record DepartmentTreeDto(
    Guid Id,
    string DepartmentName,
    Guid? ManagerId,
    string? ManagerName,
    IReadOnlyList<UserDirectoryDto> Members
);
