using EnterpriseConnect.IdentityService.Api.Authorization;
using EnterpriseConnect.IdentityService.Application.Common.Interfaces;
using EnterpriseConnect.IdentityService.Application.Features.Users.Commands;
using EnterpriseConnect.IdentityService.Application.Features.Users.DTOs;
using EnterpriseConnect.IdentityService.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.IdentityService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        var profile = await _mediator.Send(new GetUserProfileQuery(userId), cancellationToken);
        return Ok(profile);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserProfileById(Guid id, CancellationToken cancellationToken)
    {
        var profile = await _mediator.Send(new GetUserProfileQuery(id), cancellationToken);
        return Ok(profile);
    }

    [Authorize]
    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        await _mediator.Send(new UpdateProfileCommand(userId, request.FullName, request.Phone, request.Position), cancellationToken);
        return Ok(new { message = "Cập nhật thông tin hồ sơ thành công." });
    }

    [Authorize]
    [HttpPost("profile/avatar")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");

        if (file == null || file.Length == 0)
        {
            return BadRequest("Vui lòng tải lên một tệp tin ảnh.");
        }

        using var stream = file.OpenReadStream();
        var avatarUrl = await _mediator.Send(new UploadAvatarCommand(userId, stream, file.FileName), cancellationToken);

        return Ok(new { avatarUrl, message = "Cập nhật ảnh đại diện lên Cloudinary thành công!" });
    }

    [Authorize]
    [HasPermission("Users.Manage")]
    [HttpPost("{id:guid}/lock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> LockUser(Guid id, [FromBody] LockUserRequest? request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new LockUserCommand(id, request?.Reason), cancellationToken);
        return Ok(new { message = "Đã khóa tài khoản người dùng thành công và thu hồi tất cả phiên truy cập." });
    }

    [Authorize]
    [HasPermission("Users.Manage")]
    [HttpPost("{id:guid}/unlock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UnlockUser(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UnlockUserCommand(id), cancellationToken);
        return Ok(new { message = "Đã mở khóa tài khoản người dùng thành công." });
    }
}

public record UpdateProfileRequest(string FullName, string? Phone, string? Position);
public record LockUserRequest(string? Reason);
