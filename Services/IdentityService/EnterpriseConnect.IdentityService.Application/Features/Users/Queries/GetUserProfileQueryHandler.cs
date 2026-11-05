using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Users.DTOs;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.Queries;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IIdentityUnitOfWork _uow;

    public GetUserProfileQueryHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy thông tin người dùng.");

        string? deptName = null;
        if (user.DepartmentId.HasValue)
        {
            var dept = await _uow.Departments.GetByIdAsync(user.DepartmentId.Value, cancellationToken);
            deptName = dept?.DepartmentName;
        }

        var (roles, permissions) = await _uow.Users.GetRolesAndPermissionsByUserIdAsync(user.Id, cancellationToken);

        return new UserProfileDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Phone,
            user.Position,
            user.AvatarUrl,
            user.DepartmentId,
            deptName,
            user.Status,
            user.EmailConfirmed,
            roles,
            permissions
        );
    }
}
