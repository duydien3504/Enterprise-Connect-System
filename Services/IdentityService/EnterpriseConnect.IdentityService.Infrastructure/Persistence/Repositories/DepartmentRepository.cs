using EnterpriseConnect.BuildingBlocks.Infrastructure.Persistence;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.IdentityService.Infrastructure.Persistence.Repositories;

public class DepartmentRepository : EfRepository<Department>, IDepartmentRepository
{
    private readonly IdentityDbContext _identityDbContext;

    public DepartmentRepository(IdentityDbContext context) : base(context)
    {
        _identityDbContext = context;
    }

    public async Task<IReadOnlyList<DepartmentTreeDto>> GetDepartmentTreeAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _dbSet.AsNoTracking()
            .Include(d => d.Manager)
            .Include(d => d.Users.Where(u => u.Status == Domain.Enums.UserStatus.Active))
            .OrderBy(d => d.DepartmentName)
            .ToListAsync(cancellationToken);

        return departments.Select(d => new DepartmentTreeDto(
            d.Id,
            d.DepartmentName,
            d.ManagerId,
            d.Manager?.FullName,
            d.Users.Select(u => new UserDirectoryDto(
                u.Id,
                u.FullName,
                u.Email,
                u.Phone,
                u.Position,
                u.AvatarUrl,
                u.DepartmentId,
                d.DepartmentName
            )).ToList()
        )).ToList();
    }
}
