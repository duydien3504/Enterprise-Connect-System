using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record RevokeSessionByIdCommand(Guid SessionId, Guid UserId) : IRequest<bool>;
