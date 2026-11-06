using EnterpriseConnect.AdminService.Api.DTOs;
using EnterpriseConnect.AdminService.Application.Features.Moderation.Commands.RecordViolation;
using EnterpriseConnect.AdminService.Application.Features.Moderation.Commands.ResolveViolation;
using EnterpriseConnect.AdminService.Application.Features.Moderation.Queries.GetViolationById;
using EnterpriseConnect.AdminService.Application.Features.Moderation.Queries.GetViolations;
using EnterpriseConnect.AdminService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.AdminService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ModerationController : ControllerBase
{
    private readonly IMediator _mediator;

    public ModerationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("violations")]
    public async Task<IActionResult> RecordViolation([FromBody] RecordViolationRequest request)
    {
        var command = new RecordViolationCommand
        {
            MessageId = request.MessageId,
            SenderId = request.SenderId,
            ViolationType = request.ViolationType,
            Severity = request.Severity,
            ActionTaken = request.ActionTaken
        };
        var result = await _mediator.Send(command);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPatch("violations/{id:guid}/resolve")]
    public async Task<IActionResult> ResolveViolation(Guid id, [FromBody] ResolveViolationRequest request)
    {
        var command = new ResolveViolationCommand { Id = id, ActionTaken = request.ActionTaken };
        var result = await _mediator.Send(command);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpGet("violations")]
    public async Task<IActionResult> GetViolations(
        [FromQuery] ViolationType? type,
        [FromQuery] SeverityLevel? severity,
        [FromQuery] ActionTaken? action,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetViolationsQuery
        {
            ViolationType = type,
            Severity = severity,
            ActionTaken = action,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);
        return result.Succeeded ? Ok(new { items = result.Data.Items, totalCount = result.Data.TotalCount }) : BadRequest(result.Error);
    }

    [HttpGet("violations/{id:guid}")]
    public async Task<IActionResult> GetViolationById(Guid id)
    {
        var result = await _mediator.Send(new GetViolationByIdQuery(id));
        return result.Succeeded ? Ok(result.Data) : NotFound(result.Error);
    }
}
