using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Contacts.Commands;
using EnterpriseConnect.IdentityService.Application.Features.Contacts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.IdentityService.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ContactsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ContactItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContacts([FromQuery] string type = "accepted", CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        var result = await _mediator.Send(new GetUserContactsQuery(currentUserId, type), cancellationToken);
        return Ok(result);
    }

    [HttpPost("request/{receiverId:guid}")]
    [ProducesResponseType(typeof(SendContactResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendRequest([FromRoute] Guid receiverId, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        var result = await _mediator.Send(new SendContactRequestCommand(currentUserId, receiverId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("respond/{contactId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RespondRequest([FromRoute] Guid contactId, [FromBody] RespondContactBody body, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        await _mediator.Send(new RespondContactRequestCommand(contactId, currentUserId, body.Accept), cancellationToken);
        return Ok(new { message = body.Accept ? "Đã chấp nhận kết nối." : "Đã từ chối lời mời." });
    }

    [HttpDelete("{targetUserId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveContact([FromRoute] Guid targetUserId, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        var result = await _mediator.Send(new RemoveContactCommand(currentUserId, targetUserId), cancellationToken);
        return Ok(new { success = result, message = result ? "Đã xóa khỏi danh bạ." : "Không tìm thấy kết nối hợp lệ." });
    }

    [HttpPost("block/{targetUserId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BlockUser([FromRoute] Guid targetUserId, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        await _mediator.Send(new BlockUserCommand(currentUserId, targetUserId), cancellationToken);
        return Ok(new { message = "Đã chặn người dùng thành công." });
    }

    [HttpPost("unblock/{targetUserId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UnblockUser([FromRoute] Guid targetUserId, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        var result = await _mediator.Send(new UnblockUserCommand(currentUserId, targetUserId), cancellationToken);
        return Ok(new { success = result, message = result ? "Đã bỏ chặn thành công." : "Bạn không phải là người chặn người dùng này." });
    }

    [AllowAnonymous]
    [HttpGet("check-block")]
    [ProducesResponseType(typeof(BlockStatusResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckBlockStatus([FromQuery] Guid userA, [FromQuery] Guid userB, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CheckBlockStatusQuery(userA, userB), cancellationToken);
        return Ok(result);
    }
}

public record RespondContactBody(bool Accept);
