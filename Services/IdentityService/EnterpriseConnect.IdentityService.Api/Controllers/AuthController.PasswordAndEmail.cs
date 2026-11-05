using EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.IdentityService.Api.Controllers;

public partial class AuthController
{
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        await _mediator.Send(new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword), cancellationToken);
        return Ok(new { message = "Đổi mật khẩu thành công. Toàn bộ phiên làm việc cũ đã bị thu hồi." });
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var userId = await _mediator.Send(command, cancellationToken);
        return Ok(new { userId, message = "Đăng ký tài khoản thành công. Vui lòng kiểm tra email để kích hoạt tài khoản." });
    }

    [HttpGet("verify-email")]
    [HttpGet("verify_email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token, CancellationToken cancellationToken)
    {
        await _mediator.Send(new VerifyEmailCommand(token), cancellationToken);

        if (Request.Headers.Accept.ToString().Contains("text/html"))
        {
            var html = @"<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <title>Kích hoạt tài khoản thành công</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; background: #f0f2f5; }
        .card { background: white; padding: 40px; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); text-align: center; max-width: 450px; }
        .icon { font-size: 56px; color: #28a745; margin-bottom: 16px; }
        h1 { color: #333; margin-bottom: 12px; font-size: 22px; }
        p { color: #666; font-size: 15px; line-height: 1.5; margin: 0; }
    </style>
</head>
<body>
    <div class='card'>
        <div class='icon'>&#10004;</div>
        <h1>Xác thực Email thành công!</h1>
        <p>Tài khoản của bạn đã được kích hoạt thành công.<br>Bây giờ bạn đã có thể đăng nhập vào Enterprise Connect.</p>
    </div>
</body>
</html>";
            return Content(html, "text/html");
        }

        return Ok(new { message = "Xác thực email thành công! Tài khoản của bạn đã được kích hoạt." });
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok(new { message = "Nếu email hợp lệ, mã xác thực OTP 6 số đã được gửi tới hòm thư của bạn (hiệu lực 5 phút)." });
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok(new { message = "Đặt lại mật khẩu thành công. Bạn có thể sử dụng mật khẩu mới để đăng nhập." });
    }
}
