using EnterpriseConnect.BuildingBlocks.Application.Contracts;
using EnterpriseConnect.IdentityService.Domain.Entities;

namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public interface IIdentityUnitOfWork : IUnitOfWork
{
    IUserRepository Users { get; }
    IUserSessionRepository UserSessions { get; }
    IContactRepository Contacts { get; }
    IDepartmentRepository Departments { get; }
    IRepository<Role> Roles { get; }
    IRepository<Permission> Permissions { get; }
    IRepository<UserRole> UserRoles { get; }
    IRepository<RolePermission> RolePermissions { get; }
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserRolePermissionsDto> GetRolesAndPermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<UserDirectoryDto> Items, int TotalCount)> SearchUsersAsync(string? keyword, Guid? departmentId, int page, int pageSize, CancellationToken cancellationToken = default);
}

public interface IUserSessionRepository : IRepository<UserSession>
{
    Task<UserSession?> GetActiveByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task RevokeAllSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IContactRepository : IRepository<Contact>
{
    Task<Contact?> GetContactBetweenUsersAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContactItemDto>> GetUserContactsAsync(Guid userId, string filterType, CancellationToken cancellationToken = default);
    Task<(bool IsBlocked, Guid? BlockedBy)> CheckBlockStatusAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);
}

public interface IDepartmentRepository : IRepository<Department>
{
    Task<IReadOnlyList<DepartmentTreeDto>> GetDepartmentTreeAsync(CancellationToken cancellationToken = default);
}

public record UserRolePermissionsDto(IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);

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

public record DepartmentTreeDto(
    Guid Id,
    string DepartmentName,
    Guid? ManagerId,
    string? ManagerName,
    IReadOnlyList<UserDirectoryDto> Members
);
