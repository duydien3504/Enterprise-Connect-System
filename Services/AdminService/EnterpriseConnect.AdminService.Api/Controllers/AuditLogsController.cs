using EnterpriseConnect.AdminService.Api.DTOs;
using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Features.AuditLogs.Commands.RecordAuditLog;
using EnterpriseConnect.AdminService.Application.Features.AuditLogs.Queries.GetAuditLogById;
using EnterpriseConnect.AdminService.Application.Features.AuditLogs.Queries.GetAuditLogs;
using EnterpriseConnect.AdminService.Application.Features.AuditLogs.Queries.GetAuditStats;
using EnterpriseConnect.AdminService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.AdminService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public AuditLogsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpPost("record")]
    [AllowAnonymous]
    public async Task<IActionResult> Record([FromBody] RecordAuditLogRequest request)
    {
        var command = new RecordAuditLogCommand
        {
            CorrelationId = request.CorrelationId,
            ServiceName = request.ServiceName,
            ActionName = request.ActionName,
            TargetEntity = request.TargetEntity,
            TargetId = request.TargetId,
            ActorId = request.ActorId != Guid.Empty ? request.ActorId : (_currentUserService.UserId ?? Guid.Empty),
            ActorEmail = !string.IsNullOrEmpty(request.ActorEmail) ? request.ActorEmail : _currentUserService.Email,
            ActorRole = !string.IsNullOrEmpty(request.ActorRole) ? request.ActorRole : _currentUserService.Role,
            IpAddress = !string.IsNullOrEmpty(request.IpAddress) ? request.IpAddress : (_currentUserService.IpAddress ?? "unknown"),
            UserAgent = !string.IsNullOrEmpty(request.UserAgent) ? request.UserAgent : (_currentUserService.UserAgent ?? "unknown"),
            HttpMethod = request.HttpMethod,
            EndpointUrl = request.EndpointUrl,
            StatusCode = request.StatusCode,
            ExecutionDurationMs = request.ExecutionDurationMs,
            RiskLevel = request.RiskLevel,
            Details = request.Details,
            FailureReason = request.FailureReason
        };

        var result = await _mediator.Send(command);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? correlationId,
        [FromQuery] string? serviceName,
        [FromQuery] RiskLevel? riskLevel,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAuditLogsQuery
        {
            CorrelationId = correlationId,
            ServiceName = serviceName,
            RiskLevel = riskLevel,
            From = from,
            To = to,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);
        return result.Succeeded ? Ok(new { items = result.Data.Items, totalCount = result.Data.TotalCount }) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetAuditLogByIdQuery(id));
        return result.Succeeded ? Ok(result.Data) : NotFound(result.Error);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _mediator.Send(new GetAuditStatsQuery());
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Error);
    }
}
