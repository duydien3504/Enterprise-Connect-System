using EnterpriseConnect.BuildingBlocks.Infrastructure.Persistence;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using EnterpriseConnect.IdentityService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.IdentityService.Infrastructure.Persistence.Repositories;

public class UserRepository : EfRepository<User>, IUserRepository
{
    private readonly IdentityDbContext _identityDbContext;

    public UserRepository(IdentityDbContext context) : base(context)
    {
        _identityDbContext = context;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLower();
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public async Task<UserRolePermissionsDto> GetRolesAndPermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var roleNames = await _identityDbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_identityDbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.RoleName)
            .Distinct()
            .ToListAsync(cancellationToken);

        var permissionNames = await _identityDbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_identityDbContext.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp.PermissionId)
            .Join(_identityDbContext.Permissions, permId => permId, p => p.Id, (permId, p) => p.PermissionName)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new UserRolePermissionsDto(roleNames, permissionNames);
    }

    public async Task<(IReadOnlyList<UserDirectoryDto> Items, int TotalCount)> SearchUsersAsync(
        string? keyword,
        Guid? departmentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().Where(u => u.Status == UserStatus.Active);

        if (departmentId.HasValue)
        {
            query = query.Where(u => u.DepartmentId == departmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term) ||
                (u.Phone != null && u.Phone.Contains(term)) ||
                (u.Position != null && u.Position.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .Include(u => u.Department)
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDirectoryDto(
                u.Id,
                u.FullName,
                u.Email,
                u.Phone,
                u.Position,
                u.AvatarUrl,
                u.DepartmentId,
                u.Department != null ? u.Department.DepartmentName : null
            ))
            .ToListAsync(cancellationToken);

        return (users, totalCount);
    }
}
