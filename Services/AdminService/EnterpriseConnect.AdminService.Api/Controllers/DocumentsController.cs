using EnterpriseConnect.AdminService.Api.DTOs;
using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Features.Documents.Commands.DeleteDocument;
using EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UpdateDocument;
using EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UpdateDocumentStatus;
using EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UploadDocument;
using EnterpriseConnect.AdminService.Application.Features.Documents.Queries.GetDocumentById;
using EnterpriseConnect.AdminService.Application.Features.Documents.Queries.GetDocuments;
using EnterpriseConnect.AdminService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseConnect.AdminService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DocumentsController : ControllerBase
{
    private static readonly Guid FallbackUserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public DocumentsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    [RequestSizeLimit(20_971_520)]
    public async Task<IActionResult> Create([FromBody] UploadDocumentRequest request)
    {
        var command = new UploadDocumentCommand
        {
            UploaderId = _currentUserService.UserId ?? FallbackUserId,
            Title = request.Title, Description = request.Description,
            FileUrl = request.FileUrl, FileSizeBytes = request.FileSizeBytes,
            FileType = request.FileType, Version = !string.IsNullOrWhiteSpace(request.Version) ? request.Version : "1.0",
            RoleIds = request.RoleIds, DepartmentIds = request.DepartmentIds
        };
        var result = await _mediator.Send(command);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20_971_520)]
    public async Task<IActionResult> Upload([FromForm] UploadDocumentRequest request)
    {
        Stream? stream = request.File?.Length > 0 ? request.File.OpenReadStream() : null;
        var command = new UploadDocumentCommand
        {
            UploaderId = _currentUserService.UserId ?? FallbackUserId,
            Title = request.Title, Description = request.Description,
            FileUrl = request.FileUrl, FileStream = stream, FileName = request.File?.FileName,
            FileSizeBytes = request.File?.Length ?? request.FileSizeBytes,
            FileType = request.FileType, Version = !string.IsNullOrWhiteSpace(request.Version) ? request.Version : "1.0",
            RoleIds = request.RoleIds, DepartmentIds = request.DepartmentIds
        };
        var result = await _mediator.Send(command);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentRequest request)
    {
        var command = new UpdateDocumentCommand
        {
            Id = id, Title = request.Title, Description = request.Description,
            Version = request.Version, RoleIds = request.RoleIds, DepartmentIds = request.DepartmentIds
        };
        var result = await _mediator.Send(command);
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteDocumentCommand(id));
        return result.Succeeded ? Ok(result.Data) : NotFound(result.Error);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateDocumentStatusRequest request)
    {
        var result = await _mediator.Send(new UpdateDocumentStatusCommand { Id = id, Status = request.Status });
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? searchTerm, [FromQuery] DocumentStatus? status,
        [FromQuery] DocumentType? fileType, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetDocumentsQuery { SearchTerm = searchTerm, Status = status, FileType = fileType, Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query);
        return result.Succeeded ? Ok(new { items = result.Data.Items, totalCount = result.Data.TotalCount }) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDocumentByIdQuery(id));
        return result.Succeeded ? Ok(result.Data) : NotFound(result.Error);
    }
}
