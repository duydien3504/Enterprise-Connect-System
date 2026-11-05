using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Commands;

public record SendContactRequestCommand(Guid RequesterId, Guid ReceiverId) : IRequest<SendContactResultDto>;
