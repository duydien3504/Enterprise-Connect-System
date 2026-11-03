using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Commands;

public record RespondContactRequestCommand(Guid ContactId, Guid CurrentUserId, bool IsAccepted) : IRequest<bool>;

public class RespondContactRequestCommandHandler : IRequestHandler<RespondContactRequestCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;

    public RespondContactRequestCommandHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(RespondContactRequestCommand request, CancellationToken cancellationToken)
    {
        var contact = await _uow.Contacts.GetByIdAsync(request.ContactId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy lời mời kết nối.");

        if (contact.ReceiverId != request.CurrentUserId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền phản hồi lời mời này.");
        }

        if (contact.Status != ContactStatus.Pending)
        {
            throw new InvalidOperationException("Lời mời không ở trạng thái chờ phản hồi.");
        }

        contact.Status = request.IsAccepted ? ContactStatus.Accepted : ContactStatus.Rejected;
        await _uow.Contacts.UpdateAsync(contact, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}
