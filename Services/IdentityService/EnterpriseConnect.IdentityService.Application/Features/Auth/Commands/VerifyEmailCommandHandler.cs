using System.Security.Cryptography;
using System.Text;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;
    private readonly ICacheService _cacheService;

    public VerifyEmailCommandHandler(IIdentityUnitOfWork uow, ICacheService cacheService)
    {
        _uow = uow;
        _cacheService = cacheService;
    }

    public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.RawToken)));
        var cacheKey = $"email_verify:{tokenHash}";

        var userIdStr = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new ArgumentException("Mã xác thực email không hợp lệ hoặc đã hết hạn.");
        }

        var user = await _uow.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng tương ứng.");

        user.EmailConfirmed = true;
        user.Status = Domain.Enums.UserStatus.Active;
        await _uow.Users.UpdateAsync(user, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(cacheKey, cancellationToken);
        return true;
    }
}
