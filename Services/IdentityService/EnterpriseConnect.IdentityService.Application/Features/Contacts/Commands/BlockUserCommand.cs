using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Commands;

public record BlockUserCommand(Guid CurrentUserId, Guid TargetUserId) : IRequest<bool>;
