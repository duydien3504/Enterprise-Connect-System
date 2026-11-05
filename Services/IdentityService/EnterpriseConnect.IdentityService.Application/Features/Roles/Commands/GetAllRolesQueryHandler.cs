using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Roles.Commands;

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
