using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.Commands;

public record LockUserCommand(Guid UserId, string? Reason) : IRequest<bool>;
