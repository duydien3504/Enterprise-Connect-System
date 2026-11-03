using EnterpriseConnect.MessagingService.Application.Features.Conversations.Commands;
using EnterpriseConnect.MessagingService.Application.Features.Messages.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.MessagingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("direct")]
    public async Task<IActionResult> CreateDirectConversation([FromBody] CreateDirectRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateDirectConversationCommand(request.CurrentUserId, request.TargetUserId), cancellationToken);
        return Ok(new { conversationId = id });
    }

    [HttpPost("group")]
    public async Task<IActionResult> CreateGroupConversation([FromBody] CreateGroupRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateGroupConversationCommand(request.OwnerId, request.GroupName, request.MemberIds), cancellationToken);
        return Ok(new { conversationId = id });
    }

    [HttpPost("{id:guid}/summarize")]
    public async Task<IActionResult> SummarizeConversation(Guid id, [FromQuery] Guid userId, [FromQuery] int count = 50, CancellationToken cancellationToken = default)
    {
        var summary = await _mediator.Send(new SummarizeConversationCommand(id, userId, count), cancellationToken);
        return Ok(summary);
    }
}

public record CreateDirectRequest(Guid CurrentUserId, Guid TargetUserId);
public record CreateGroupRequest(Guid OwnerId, string GroupName, List<Guid> MemberIds);
