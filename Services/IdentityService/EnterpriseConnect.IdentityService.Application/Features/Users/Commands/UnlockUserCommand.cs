using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.Commands;

public record UnlockUserCommand(Guid UserId) : IRequest<bool>;
