using EnterpriseConnect.MessagingService.Api.Models;
using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;
using EnterpriseConnect.MessagingService.Application.Features.Messages.DTOs;
using EnterpriseConnect.MessagingService.Application.Features.Messages.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.MessagingService.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IToxicDetectionService _toxicService;
    private readonly ICurrentUserService _currentUserService;

    public MessagesController(
        IMediator mediator, 
        IToxicDetectionService toxicService,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _toxicService = toxicService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        var command = new SendMessageCommand(request.ConversationId, currentUserId, request.Content, request.Type, request.ReplyToId);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsToxicBlocked)
        {
            return BadRequest(new { message = $"Tin nhắn bị chặn: {result.ToxicReason}" });
        }
        return Ok(result);
    }

    [HttpGet("conversation/{conversationId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<MessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversationMessages(
        Guid conversationId, 
        [FromQuery] int skip = 0, 
        [FromQuery] int take = 50, 
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        var result = await _mediator.Send(new GetConversationMessagesQuery(conversationId, currentUserId, skip, take), cancellationToken);
        return Ok(result);
    }

    [HttpGet("conversation/{conversationId:guid}/pinned")]
    [ProducesResponseType(typeof(IReadOnlyList<MessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPinnedMessages(Guid conversationId, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        var result = await _mediator.Send(new GetPinnedMessagesQuery(conversationId, currentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reaction")]
    public async Task<IActionResult> ReactMessage(Guid id, [FromBody] ReactMessageRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        await _mediator.Send(new ReactMessageCommand(id, currentUserId, request.Emoji), cancellationToken);
        return Ok(new { message = "Đã cập nhật cảm xúc tin nhắn." });
    }

    [HttpPost("{id:guid}/pin")]
    public async Task<IActionResult> TogglePin(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        var isPinned = await _mediator.Send(new TogglePinMessageCommand(id, currentUserId), cancellationToken);
        return Ok(new { isPinned, message = isPinned ? "Đã ghim tin nhắn." : "Đã bỏ ghim tin nhắn." });
    }

    [AllowAnonymous]
    [HttpPost("check-toxic")]
    public async Task<IActionResult> CheckToxic([FromBody] CheckToxicRequest request, CancellationToken cancellationToken)
    {
        var result = await _toxicService.AnalyzeContentAsync(request.Content, cancellationToken);
        return Ok(result);
    }
}
