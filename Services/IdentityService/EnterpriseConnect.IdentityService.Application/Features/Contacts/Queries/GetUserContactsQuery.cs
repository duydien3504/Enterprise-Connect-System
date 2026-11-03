using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Queries;

public record GetUserContactsQuery(Guid UserId, string FilterType) : IRequest<IReadOnlyList<ContactItemDto>>;

public class GetUserContactsQueryHandler : IRequestHandler<GetUserContactsQuery, IReadOnlyList<ContactItemDto>>
{
    private readonly IIdentityUnitOfWork _uow;

    public GetUserContactsQueryHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IReadOnlyList<ContactItemDto>> Handle(GetUserContactsQuery request, CancellationToken cancellationToken)
    {
        return await _uow.Contacts.GetUserContactsAsync(request.UserId, request.FilterType, cancellationToken);
    }
}
