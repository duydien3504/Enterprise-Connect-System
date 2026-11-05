using EnterpriseConnect.MediaService.Api.DTOs;
using EnterpriseConnect.MediaService.Application.Common.Interfaces;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.EndCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.InitiateCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.InviteParticipant;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.RespondCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.UpdateMediaState;
using EnterpriseConnect.MediaService.Application.Features.Calls.Queries.GetCallHistory;
using EnterpriseConnect.MediaService.Application.Features.Calls.Queries.GetIceServers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.MediaService.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CallsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public CallsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new UnauthorizedAccessException("User is not authenticated.");

    [HttpPost("initiate")]
    public async Task<IActionResult> InitiateCall([FromBody] InitiateCallRequest request)
    {
        var result = await _mediator.Send(new InitiateCallCommand(
            request.ConversationId, CurrentUserId, request.CallType, request.Scope, request.ParticipantIds));
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/respond")]
    public async Task<IActionResult> RespondCall(Guid id, [FromBody] RespondCallRequest request)
    {
        var result = await _mediator.Send(new RespondCallCommand(id, CurrentUserId, request.Accept, request.DeclineReason));
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/end")]
    public async Task<IActionResult> EndCall(Guid id, [FromBody] EndCallRequest request)
    {
        var result = await _mediator.Send(new EndCallCommand(id, CurrentUserId, request.Reason));
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}/media-state")]
    public async Task<IActionResult> UpdateMediaState(Guid id, [FromBody] UpdateMediaStateRequest request)
    {
        var result = await _mediator.Send(new UpdateMediaStateCommand(
            id, CurrentUserId, request.IsMuted, request.IsCameraOn, request.IsScreenSharing));
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/invite")]
    public async Task<IActionResult> InviteParticipants(Guid id, [FromBody] InviteParticipantsRequest request)
    {
        var result = await _mediator.Send(new InviteParticipantCommand(id, CurrentUserId, request.UserIds));
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetCallHistory([FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var result = await _mediator.Send(new GetCallHistoryQuery(CurrentUserId, skip, take));
        return Ok(result.Data);
    }

    [HttpGet("ice-servers")]
    public async Task<IActionResult> GetIceServers()
    {
        var result = await _mediator.Send(new GetIceServersQuery());
        return Ok(result.Data);
    }
}
