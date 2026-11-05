using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Departments.Queries;
using EnterpriseConnect.IdentityService.Application.Features.Users.DTOs;
using EnterpriseConnect.IdentityService.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.IdentityService.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DirectoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public DirectoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("tree")]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentTreeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizationTree(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDepartmentTreeQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(SearchUsersResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchUsers(
        [FromQuery] string? keyword,
        [FromQuery] Guid? departmentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new SearchUsersQuery(keyword, departmentId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}
