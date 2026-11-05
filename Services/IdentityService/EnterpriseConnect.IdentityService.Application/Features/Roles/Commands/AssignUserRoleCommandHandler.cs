using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Entities;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Roles.Commands;

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
