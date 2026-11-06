using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UpdateDocument;

public class UpdateDocumentCommand : IRequest<Result<DocumentDto>>
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
    public List<Guid>? RoleIds { get; set; }
    public List<Guid>? DepartmentIds { get; set; }
}
