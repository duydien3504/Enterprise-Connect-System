using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Commands;

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
            return await _uow.Contacts.HardDeleteBetweenUsersAsync(request.CurrentUserId, request.TargetUserId, cancellationToken);
        }

        return false;
    }
}
