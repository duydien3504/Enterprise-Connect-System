using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Entities;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Roles.Commands;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Guid>
{
    private readonly IIdentityUnitOfWork _uow;

    public CreateRoleCommandHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = new Role
        {
            RoleName = request.Name.Trim(),
            Description = request.Description?.Trim()
        };

        await _uow.Roles.AddAsync(role, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        if (request.PermissionNames != null && request.PermissionNames.Any())
        {
            var allPerms = await _uow.Permissions.GetAllAsync(cancellationToken);
            foreach (var permName in request.PermissionNames)
            {
                var perm = allPerms.FirstOrDefault(p => string.Equals(p.PermissionName, permName, StringComparison.OrdinalIgnoreCase));
                if (perm != null)
                {
                    await _uow.RolePermissions.AddAsync(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = perm.Id
                    }, cancellationToken);
                }
            }
            await _uow.SaveChangesAsync(cancellationToken);
        }

        return role.Id;
    }
}
