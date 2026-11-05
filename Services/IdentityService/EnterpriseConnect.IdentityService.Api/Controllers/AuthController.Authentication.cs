using EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;
using EnterpriseConnect.IdentityService.Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.IdentityService.Api.Controllers;

public partial class AuthController
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var loginWithMeta = command with
        {
            IpAddress = ipAddress,
            DeviceInfo = userAgent
        };

        var response = await _mediator.Send(loginWithMeta, cancellationToken);
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var refreshWithMeta = command with
        {
            IpAddress = ipAddress,
            DeviceInfo = userAgent
        };

        var response = await _mediator.Send(refreshWithMeta, cancellationToken);
        return Ok(response);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok(new { message = "Đăng xuất thành công." });
    }

    [Authorize]
    [HttpPost("logout-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAllDevices(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        await _mediator.Send(new LogoutAllDevicesCommand(userId), cancellationToken);
        return Ok(new { message = "Đã đăng xuất khỏi toàn bộ các thiết bị thành công." });
    }
}
