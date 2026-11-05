using EnterpriseConnect.RemoteService.Api.DTOs;
using EnterpriseConnect.RemoteService.Application.Common.Interfaces;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.ApproveSession;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.CreateSession;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.JoinSession;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RequestPermission;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RespondPermission;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.RevokePermission;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Commands.TerminateSession;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Queries.GetSessionByCode;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Queries.GetSessionDetails;
using EnterpriseConnect.RemoteService.Application.Features.RemoteSessions.Queries.GetSessionHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.RemoteService.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RemoteSessionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public RemoteSessionsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    private Guid CurrentUserId => _currentUser.UserId
        ?? throw new UnauthorizedAccessException("User is not authenticated.");

    [HttpPost("create")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        var result = await _mediator.Send(new CreateSessionCommand(CurrentUserId, request.ExpirationMinutes));
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinSession([FromBody] JoinSessionRequest request)
    {
        var result = await _mediator.Send(new JoinSessionCommand(request.SessionCode, CurrentUserId));
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApproveSession(Guid id, [FromBody] ApproveSessionRequest request)
    {
        var result = await _mediator.Send(new ApproveSessionCommand(id, CurrentUserId, request.Accept, request.RejectionReason));
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/permissions/request")]
    public async Task<IActionResult> RequestPermission(Guid id, [FromBody] RequestPermissionRequest request)
    {
        var result = await _mediator.Send(new RequestPermissionCommand(id, CurrentUserId, request.PermissionType));
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/permissions/respond")]
    public async Task<IActionResult> RespondPermission(Guid id, [FromBody] RespondPermissionRequest request)
    {
        var result = await _mediator.Send(new RespondPermissionCommand(id, CurrentUserId, request.PermissionType, request.Grant));
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/permissions/revoke")]
    public async Task<IActionResult> RevokePermission(Guid id, [FromBody] RequestPermissionRequest request)
    {
        var result = await _mediator.Send(new RevokePermissionCommand(id, CurrentUserId, request.PermissionType));
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/terminate")]
    public async Task<IActionResult> TerminateSession(Guid id, [FromBody] TerminateSessionRequest request)
    {
        var result = await _mediator.Send(new TerminateSessionCommand(id, CurrentUserId, request.Reason));
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetSessionByCode(string code)
    {
        var result = await _mediator.Send(new GetSessionByCodeQuery(code));
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSessionDetails(Guid id)
    {
        var result = await _mediator.Send(new GetSessionDetailsQuery(id));
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetSessionHistory([FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var result = await _mediator.Send(new GetSessionHistoryQuery(CurrentUserId, skip, take));
        return Ok(result.Data);
    }
}
