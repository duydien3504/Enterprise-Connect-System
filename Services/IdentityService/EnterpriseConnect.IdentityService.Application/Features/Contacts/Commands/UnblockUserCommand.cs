using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Commands;

public record UnblockUserCommand(Guid CurrentUserId, Guid TargetUserId) : IRequest<bool>;

public class UnblockUserCommandHandler : IRequestHandler<UnblockUserCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;

    public UnblockUserCommandHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(UnblockUserCommand request, CancellationToken cancellationToken)
    {
        var contact = await _uow.Contacts.GetContactBetweenUsersAsync(request.CurrentUserId, request.TargetUserId, cancellationToken);

        // Chỉ cho phép bỏ chặn nếu chính mình là người đã chặn (RequesterId == CurrentUserId)
        if (contact != null && contact.Status == ContactStatus.Blocked && contact.RequesterId == request.CurrentUserId)
        {
            await _uow.Contacts.DeleteAsync(contact, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }

        return false;
    }
}
