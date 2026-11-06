using EnterpriseConnect.AdminService.Api.DTOs;
using EnterpriseConnect.AdminService.Application.Features.System.Commands.UpdateSystemSetting;
using EnterpriseConnect.AdminService.Application.Features.System.Queries.GetDashboardOverview;
using EnterpriseConnect.AdminService.Application.Features.System.Queries.GetSystemSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.AdminService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class SystemController : ControllerBase
{
    private readonly IMediator _mediator;

    public SystemController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _mediator.Send(new GetDashboardOverviewQuery());
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings([FromQuery] string? groupName)
    {
        var result = await _mediator.Send(new GetSystemSettingsQuery { GroupName = groupName });
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("settings")]
    public async Task<IActionResult> UpdateSetting([FromBody] UpdateSystemSettingRequest request)
    {
        var command = new UpdateSystemSettingCommand
        {
            SettingKey = request.SettingKey,
            SettingValue = request.SettingValue,
            GroupName = request.GroupName,
            Description = request.Description,
            IsPublic = request.IsPublic
        };
        var result = await _mediator.Send(command);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Error);
    }
}
