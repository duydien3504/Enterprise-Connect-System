using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.IdentityService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }
}

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
