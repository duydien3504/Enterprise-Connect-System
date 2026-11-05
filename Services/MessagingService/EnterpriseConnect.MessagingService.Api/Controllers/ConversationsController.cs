using EnterpriseConnect.MessagingService.Api.Models;
using EnterpriseConnect.MessagingService.Application.Common.Interfaces;
using EnterpriseConnect.MessagingService.Application.Features.Conversations.Commands;
using EnterpriseConnect.MessagingService.Application.Features.Conversations.DTOs;
using EnterpriseConnect.MessagingService.Application.Features.Conversations.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.MessagingService.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ConversationsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ConversationItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyConversations(CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        var result = await _mediator.Send(new GetUserConversationsQuery(currentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("direct")]
    public async Task<IActionResult> CreateDirectConversation([FromBody] CreateDirectRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        var id = await _mediator.Send(new CreateDirectConversationCommand(currentUserId, request.TargetUserId), cancellationToken);
        return Ok(new { conversationId = id });
    }

    [HttpPost("group")]
    public async Task<IActionResult> CreateGroupConversation([FromBody] CreateGroupRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        var id = await _mediator.Send(new CreateGroupConversationCommand(currentUserId, request.GroupName, request.MemberIds), cancellationToken);
        return Ok(new { conversationId = id });
    }

    [HttpPost("{id:guid}/summarize")]
    public async Task<IActionResult> SummarizeConversation(
        Guid id, 
        [FromQuery] int count = 50, 
        [FromQuery] string language = "vi", 
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        var summary = await _mediator.Send(new SummarizeConversationCommand(id, currentUserId, count, language), cancellationToken);
        return Ok(summary);
    }

    [HttpGet("{id:guid}/members")]
    [ProducesResponseType(typeof(IReadOnlyList<ConversationMemberDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversationMembers(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        var result = await _mediator.Send(new GetConversationMembersQuery(id, currentUserId), cancellationToken);
        return Ok(result);
    }
}
