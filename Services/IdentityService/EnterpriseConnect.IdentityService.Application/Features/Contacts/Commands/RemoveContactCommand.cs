using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Commands;

public record RemoveContactCommand(Guid CurrentUserId, Guid TargetUserId) : IRequest<bool>;
