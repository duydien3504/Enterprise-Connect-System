using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.Commands;

public record UpdateProfileCommand(
    Guid UserId,
    string FullName,
    string? Phone,
    string? Position
) : IRequest<bool>;
