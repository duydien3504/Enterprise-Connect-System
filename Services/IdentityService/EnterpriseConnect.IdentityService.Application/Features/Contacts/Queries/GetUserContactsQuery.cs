using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Queries;

public record GetUserContactsQuery(Guid UserId, string FilterType) : IRequest<IReadOnlyList<ContactItemDto>>;
