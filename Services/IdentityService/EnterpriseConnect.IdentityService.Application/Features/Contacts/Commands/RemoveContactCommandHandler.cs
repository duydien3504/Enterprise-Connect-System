using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Commands;

public class RemoveContactCommandHandler : IRequestHandler<RemoveContactCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;

    public RemoveContactCommandHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(RemoveContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _uow.Contacts.GetContactBetweenUsersAsync(request.CurrentUserId, request.TargetUserId, cancellationToken);

        if (contact != null && contact.Status == ContactStatus.Accepted)
        {
            return await _uow.Contacts.HardDeleteBetweenUsersAsync(request.CurrentUserId, request.TargetUserId, cancellationToken);
        }

        return false;
    }
}
