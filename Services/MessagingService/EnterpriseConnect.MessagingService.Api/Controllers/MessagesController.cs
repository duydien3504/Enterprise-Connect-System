using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.MessagingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IToxicDetectionService _toxicService;

    public MessagesController(IMediator mediator, IToxicDetectionService toxicService)
    {
        _mediator = mediator;
        _toxicService = toxicService;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsToxicBlocked)
        {
            return BadRequest(new { message = $"Tin nhắn bị chặn: {result.ToxicReason}" });
        }
        return Ok(result);
    }

    [HttpPost("{id:guid}/reaction")]
    public async Task<IActionResult> ReactMessage(Guid id, [FromBody] ReactRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReactMessageCommand(id, request.UserId, request.Emoji), cancellationToken);
        return Ok(new { message = "Đã cập nhật cảm xúc tin nhắn." });
    }

    [HttpPost("{id:guid}/pin")]
    public async Task<IActionResult> TogglePin(Guid id, [FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        var isPinned = await _mediator.Send(new TogglePinMessageCommand(id, userId), cancellationToken);
        return Ok(new { isPinned, message = isPinned ? "Đã ghim tin nhắn." : "Đã bỏ ghim tin nhắn." });
    }

    [HttpPost("check-toxic")]
    public async Task<IActionResult> CheckToxic([FromBody] CheckToxicRequest request, CancellationToken cancellationToken)
    {
        var result = await _toxicService.AnalyzeContentAsync(request.Content, cancellationToken);
        return Ok(result);
    }
}

public record ReactRequest(Guid UserId, string Emoji);
public record CheckToxicRequest(string Content);
