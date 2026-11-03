using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Entities;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Roles.Commands;

public record RoleDto(Guid Id, string Name, string? Description, IReadOnlyList<string> Permissions);

public record GetAllRolesQuery : IRequest<IReadOnlyList<RoleDto>>;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, IReadOnlyList<RoleDto>>
{
    private readonly IIdentityUnitOfWork _uow;

    public GetAllRolesQueryHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IReadOnlyList<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _uow.Roles.GetAllAsync(cancellationToken);
        var rolePermissions = await _uow.RolePermissions.GetAllAsync(cancellationToken);
        var permissions = await _uow.Permissions.GetAllAsync(cancellationToken);

        var permLookup = permissions.ToDictionary(p => p.Id, p => p.PermissionName);

        var result = new List<RoleDto>();
        foreach (var r in roles)
        {
            var rolePermNames = rolePermissions
                .Where(rp => rp.RoleId == r.Id && permLookup.ContainsKey(rp.PermissionId))
                .Select(rp => permLookup[rp.PermissionId])
                .ToList();

            result.Add(new RoleDto(r.Id, r.RoleName, r.Description, rolePermNames));
        }

        return result;
    }
}

public record CreateRoleCommand(string Name, string? Description, List<string>? PermissionNames) : IRequest<Guid>;

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

public record AssignUserRoleCommand(Guid UserId, Guid RoleId) : IRequest<bool>;

public class AssignUserRoleCommandHandler : IRequestHandler<AssignUserRoleCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;

    public AssignUserRoleCommandHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(AssignUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        var role = await _uow.Roles.GetByIdAsync(request.RoleId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy vai trò (Role).");

        var existing = await _uow.UserRoles.FindAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken);
        if (existing.Any())
        {
            return true; // Đã gán trước đó
        }

        await _uow.UserRoles.AddAsync(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        }, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}
