using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Commands;

public record UnblockUserCommand(Guid CurrentUserId, Guid TargetUserId) : IRequest<bool>;
