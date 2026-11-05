using EnterpriseConnect.BuildingBlocks.Application.Contracts;
using EnterpriseConnect.IdentityService.Domain.Entities;

namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserRolePermissionsDto> GetRolesAndPermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<UserDirectoryDto> Items, int TotalCount)> SearchUsersAsync(string? keyword, Guid? departmentId, int page, int pageSize, CancellationToken cancellationToken = default);
}
