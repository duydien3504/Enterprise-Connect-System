using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Contacts.Commands;

public class SendContactRequestCommandHandler : IRequestHandler<SendContactRequestCommand, SendContactResultDto>
{
    private readonly IIdentityUnitOfWork _uow;

    public SendContactRequestCommandHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<SendContactResultDto> Handle(SendContactRequestCommand request, CancellationToken cancellationToken)
    {
        if (request.RequesterId == request.ReceiverId)
        {
            throw new ArgumentException("Bạn không thể gửi lời mời kết nối cho chính mình.");
        }

        var receiver = await _uow.Users.GetByIdAsync(request.ReceiverId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng nhận lời mời.");

        if (receiver.Status != UserStatus.Active)
        {
            throw new ArgumentException("Người dùng hiện không hoạt động hoặc tài khoản bị khóa.");
        }

        var existingContact = await _uow.Contacts.GetContactBetweenUsersAsync(request.RequesterId, request.ReceiverId, cancellationToken);

        if (existingContact != null)
        {
            // Case 1: Đối phương đang Block mình
            if (existingContact.Status == ContactStatus.Blocked && existingContact.RequesterId == request.ReceiverId)
            {
                throw new ArgumentException("Không thể gửi lời mời kết nối tới người dùng này.");
            }

            // Case 2: Mình đang Block đối phương -> Gỡ block và chuyển thành Pending
            if (existingContact.Status == ContactStatus.Blocked && existingContact.RequesterId == request.RequesterId)
            {
                existingContact.Status = ContactStatus.Pending;
                await _uow.Contacts.UpdateAsync(existingContact, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);
                return new SendContactResultDto(existingContact.Id, (int)ContactStatus.Pending, "Đã gửi lời mời kết nối.");
            }

            // Case 3: Đã kết nối trước đó
            if (existingContact.Status == ContactStatus.Accepted)
            {
                return new SendContactResultDto(existingContact.Id, (int)ContactStatus.Accepted, "Hai người đã là bạn bè kết nối.");
            }

            // Case 4: Đã gửi lời mời và đang chờ
            if (existingContact.Status == ContactStatus.Pending && existingContact.RequesterId == request.RequesterId)
            {
                return new SendContactResultDto(existingContact.Id, (int)ContactStatus.Pending, "Lời mời kết nối đang chờ phản hồi.");
            }

            // Case 5: Đối phương đã gửi lời mời cho mình trước đó -> Auto-Accept ngay!
            if (existingContact.Status == ContactStatus.Pending && existingContact.ReceiverId == request.RequesterId)
            {
                existingContact.Status = ContactStatus.Accepted;
                await _uow.Contacts.UpdateAsync(existingContact, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);
                return new SendContactResultDto(existingContact.Id, (int)ContactStatus.Accepted, "Đối phương cũng đã gửi lời mời cho bạn. Kết nối thành công!");
            }

            // Case 6: Từng Rejected -> Cho phép gửi lại
            existingContact.RequesterId = request.RequesterId;
            existingContact.ReceiverId = request.ReceiverId;
            existingContact.Status = ContactStatus.Pending;
            await _uow.Contacts.UpdateAsync(existingContact, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            return new SendContactResultDto(existingContact.Id, (int)ContactStatus.Pending, "Đã gửi lại lời mời kết nối.");
        }

        // Tạo mới lời mời
        var newContact = new Contact
        {
            RequesterId = request.RequesterId,
            ReceiverId = request.ReceiverId,
            Status = ContactStatus.Pending
        };

        await _uow.Contacts.AddAsync(newContact, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new SendContactResultDto(newContact.Id, (int)ContactStatus.Pending, "Đã gửi lời mời kết nối thành công.");
    }
}
