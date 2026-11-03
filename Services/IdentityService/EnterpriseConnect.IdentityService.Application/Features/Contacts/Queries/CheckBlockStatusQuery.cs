using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Queries;

public record CheckBlockStatusQuery(Guid UserA, Guid UserB) : IRequest<BlockStatusResultDto>;

public record BlockStatusResultDto(bool IsBlocked, Guid? BlockedBy);

public class CheckBlockStatusQueryHandler : IRequestHandler<CheckBlockStatusQuery, BlockStatusResultDto>
{
    private readonly IIdentityUnitOfWork _uow;

    public CheckBlockStatusQueryHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<BlockStatusResultDto> Handle(CheckBlockStatusQuery request, CancellationToken cancellationToken)
    {
        var (isBlocked, blockedBy) = await _uow.Contacts.CheckBlockStatusAsync(request.UserA, request.UserB, cancellationToken);
        return new BlockStatusResultDto(isBlocked, blockedBy);
    }
}
