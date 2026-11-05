using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Users.Commands;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;

    public UpdateProfileCommandHandler(IIdentityUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        user.FullName = request.FullName.Trim();
        user.Phone = request.Phone?.Trim();
        user.Position = request.Position?.Trim();

        await _uow.Users.UpdateAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}
