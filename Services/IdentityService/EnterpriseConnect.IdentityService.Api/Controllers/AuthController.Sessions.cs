using EnterpriseConnect.IdentityService.Application.Features.Auth.Commands;
using EnterpriseConnect.IdentityService.Application.Features.Auth.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.IdentityService.Api.Controllers;

public partial class AuthController
{
    [Authorize]
    [HttpGet("sessions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        var sessions = await _mediator.Send(new GetUserSessionsQuery(userId), cancellationToken);
        return Ok(sessions);
    }

    [Authorize]
    [HttpDelete("sessions/{sessionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokeSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        await _mediator.Send(new RevokeSessionByIdCommand(sessionId, userId), cancellationToken);
        return Ok(new { message = "Đã ngắt phiên đăng nhập thành công." });
    }
}
