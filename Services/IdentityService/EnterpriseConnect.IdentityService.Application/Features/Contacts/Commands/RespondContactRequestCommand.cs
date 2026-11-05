using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Commands;

public record RespondContactRequestCommand(Guid ContactId, Guid CurrentUserId, bool IsAccepted) : IRequest<bool>;
