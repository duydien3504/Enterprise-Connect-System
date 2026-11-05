using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Commands;

public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;

    public BlockUserCommandHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == request.TargetUserId)
        {
            throw new ArgumentException("Bạn không thể chặn chính mình.");
        }

        var contact = await _uow.Contacts.GetContactBetweenUsersAsync(request.CurrentUserId, request.TargetUserId, cancellationToken);

        if (contact != null)
        {
            contact.RequesterId = request.CurrentUserId; // Người chặn luôn là Requester
            contact.ReceiverId = request.TargetUserId;
            contact.Status = ContactStatus.Blocked;
            await _uow.Contacts.UpdateAsync(contact, cancellationToken);
        }
        else
        {
            contact = new Contact
            {
                RequesterId = request.CurrentUserId,
                ReceiverId = request.TargetUserId,
                Status = ContactStatus.Blocked
            };
            await _uow.Contacts.AddAsync(contact, cancellationToken);
        }

        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}
