using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.IdentityService.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PresenceController : ControllerBase
{
    private readonly IPresenceService _presenceService;
    private readonly ICurrentUserService _currentUserService;

    public PresenceController(IPresenceService presenceService, ICurrentUserService currentUserService)
    {
        _presenceService = presenceService;
        _currentUserService = currentUserService;
    }

    [HttpPost("heartbeat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Heartbeat([FromBody] HeartbeatRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        await _presenceService.SetPresenceAsync(currentUserId, request.Status, request.DeviceType, cancellationToken);
        return Ok(new { success = true });
    }

    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(PresenceStatus), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPresence([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var status = await _presenceService.GetPresenceAsync(userId, cancellationToken);
        return Ok(new { userId, status });
    }

    [HttpPost("batch")]
    [ProducesResponseType(typeof(Dictionary<Guid, PresenceStatus>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBatchPresence([FromBody] List<Guid> userIds, CancellationToken cancellationToken)
    {
        var result = await _presenceService.GetBatchPresenceAsync(userIds, cancellationToken);
        return Ok(result);
    }
}

public record HeartbeatRequest(PresenceStatus Status = PresenceStatus.Online, DeviceType DeviceType = DeviceType.Web);
