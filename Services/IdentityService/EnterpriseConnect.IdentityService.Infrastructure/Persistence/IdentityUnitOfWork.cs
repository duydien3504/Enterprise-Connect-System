using EnterpriseConnect.BuildingBlocks.Application.Contracts;
using EnterpriseConnect.BuildingBlocks.Infrastructure.Persistence;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Infrastructure.Persistence.Context;
using EnterpriseConnect.IdentityService.Infrastructure.Persistence.Repositories;

namespace EnterpriseConnect.IdentityService.Infrastructure.Persistence;

public class IdentityUnitOfWork : IIdentityUnitOfWork
{
    private readonly IdentityDbContext _context;
    private IUserRepository? _users;
    private IUserSessionRepository? _userSessions;
    private IContactRepository? _contacts;
    private IDepartmentRepository? _departments;
    private IRepository<Role>? _roles;
    private IRepository<Permission>? _permissions;
    private IRepository<UserRole>? _userRoles;
    private IRepository<RolePermission>? _rolePermissions;

    public IdentityUnitOfWork(IdentityDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IUserSessionRepository UserSessions => _userSessions ??= new UserSessionRepository(_context);
    public IContactRepository Contacts => _contacts ??= new ContactRepository(_context);
    public IDepartmentRepository Departments => _departments ??= new DepartmentRepository(_context);
    public IRepository<Role> Roles => _roles ??= new EfRepository<Role>(_context);
    public IRepository<Permission> Permissions => _permissions ??= new EfRepository<Permission>(_context);
    public IRepository<UserRole> UserRoles => _userRoles ??= new EfRepository<UserRole>(_context);
    public IRepository<RolePermission> RolePermissions => _rolePermissions ??= new EfRepository<RolePermission>(_context);

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class, BuildingBlocks.Domain.Common.IEntity
    {
        return _context.Repository<TEntity>();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _context.BeginTransactionAsync(cancellationToken);
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _context.CommitTransactionAsync(cancellationToken);
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _context.RollbackTransactionAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
