using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record RegisterUserCommand(
    string Email,
    string Password,
    string FullName,
    Guid? DepartmentId = null,
    string? Position = null,
    string? Phone = null
) : IRequest<Guid>;
