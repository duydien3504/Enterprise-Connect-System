using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<bool>;
