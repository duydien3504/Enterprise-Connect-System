using EnterpriseConnect.BuildingBlocks.Application.Contracts;
using EnterpriseConnect.IdentityService.Domain.Entities;

namespace EnterpriseConnect.IdentityService.Application.Common.Interfaces;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<IReadOnlyList<DepartmentTreeDto>> GetDepartmentTreeAsync(CancellationToken cancellationToken = default);
}
