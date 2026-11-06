using EnterpriseConnect.MessagingService.Application.Features.Knowledge.Commands.SyncKnowledgeChunks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.MessagingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KnowledgeController : ControllerBase
{
    private readonly IMediator _mediator;

    public KnowledgeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("sync")]
    [AllowAnonymous]
    public async Task<IActionResult> Sync([FromBody] SyncKnowledgeChunksCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
