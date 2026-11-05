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
