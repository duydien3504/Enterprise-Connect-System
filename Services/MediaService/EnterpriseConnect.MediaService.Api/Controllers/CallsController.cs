using EnterpriseConnect.MediaService.Api.DTOs;
using EnterpriseConnect.MediaService.Application.Common.Models;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.EndCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.InitiateCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.InviteParticipant;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.RespondCall;
using EnterpriseConnect.MediaService.Application.Features.Calls.Commands.UpdateMediaState;
using EnterpriseConnect.MediaService.Application.Features.Calls.Queries.GetCallHistory;
using EnterpriseConnect.MediaService.Application.Features.Calls.Queries.GetIceServers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.MediaService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CallsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CallsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("initiate")]
    public async Task<IActionResult> InitiateCall([FromBody] InitiateCallRequest request)
    {
        var result = await _mediator.Send(new InitiateCallCommand(
            request.ConversationId,
            request.InitiatorId,
            request.CallType,
            request.Scope,
            request.ParticipantIds));

        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpPost("{id:guid}/respond")]
    public async Task<IActionResult> RespondCall(Guid id, [FromBody] RespondCallRequest request)
    {
        var result = await _mediator.Send(new RespondCallCommand(
            id,
            request.UserId,
            request.Accept,
            request.DeclineReason));

        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpPost("{id:guid}/end")]
    public async Task<IActionResult> EndCall(Guid id, [FromBody] EndCallRequest request)
    {
        var result = await _mediator.Send(new EndCallCommand(
            id,
            request.UserId,
            request.Reason));

        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpPut("{id:guid}/media-state")]
    public async Task<IActionResult> UpdateMediaState(Guid id, [FromBody] UpdateMediaStateRequest request)
    {
        var result = await _mediator.Send(new UpdateMediaStateCommand(
            id,
            request.UserId,
            request.IsMuted,
            request.IsCameraOn,
            request.IsScreenSharing));

        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpPost("{id:guid}/invite")]
    public async Task<IActionResult> InviteParticipants(Guid id, [FromBody] InviteParticipantsRequest request)
    {
        var result = await _mediator.Send(new InviteParticipantCommand(
            id,
            request.InviterId,
            request.UserIds));

        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Data);
    }

    [HttpGet("history/{userId:guid}")]
    public async Task<IActionResult> GetCallHistory(Guid userId, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var result = await _mediator.Send(new GetCallHistoryQuery(userId, skip, take));
        return Ok(result.Data);
    }

    [HttpGet("ice-servers")]
    public async Task<IActionResult> GetIceServers()
    {
        var result = await _mediator.Send(new GetIceServersQuery());
        return Ok(result.Data);
    }
}
