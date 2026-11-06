using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Queries.GetDocuments;

public class GetDocumentsQuery : IRequest<Result<(List<DocumentDto> Items, long TotalCount)>>
{
    public string? SearchTerm { get; set; }
    public DocumentStatus? Status { get; set; }
    public DocumentType? FileType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
