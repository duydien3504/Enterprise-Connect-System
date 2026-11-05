using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Queries;

public record CheckBlockStatusQuery(Guid UserA, Guid UserB) : IRequest<BlockStatusResultDto>;
