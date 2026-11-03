using System.Security.Cryptography;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Entities;
using EnterpriseConnect.IdentityService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;

public record ForgotPasswordCommand(string Email) : IRequest<bool>;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IIdentityUnitOfWork _uow;
    private readonly ICacheService _cacheService;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordCommandHandler(
        IIdentityUnitOfWork uow,
        ICacheService cacheService,
        IEmailSender emailSender)
    {
        _uow = uow;
        _cacheService = cacheService;
        _emailSender = emailSender;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || user.Status != UserStatus.Active)
        {
            // Trả về true giả lập để chống Email Enumeration Attack
            return true;
        }

        // Sinh OTP 6 số
        var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var cacheKey = $"pwd_reset_otp:{request.Email.ToLower().Trim()}";

        // Lưu vào Cache Redis với TTL 5 phút
        await _cacheService.SetAsync(cacheKey, otp, TimeSpan.FromMinutes(5), cancellationToken);

        // Gửi email OTP
        var bodyHtml = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; color: #333;'>
                <h2>Yêu cầu đặt lại mật khẩu</h2>
                <p>Chào bạn,</p>
                <p>Hệ thống Enterprise Connect nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                <p>Mã xác thực OTP của bạn là:</p>
                <div style='font-size: 28px; font-weight: bold; color: #007bff; letter-spacing: 5px; margin: 15px 0;'>{otp}</div>
                <p>Mã này có hiệu lực trong vòng <strong>5 phút</strong>. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>
            </div>";

        await _emailSender.SendEmailAsync(request.Email, "[Enterprise Connect] Mã xác thực đặt lại mật khẩu", bodyHtml, cancellationToken);

        return true;
    }
}
