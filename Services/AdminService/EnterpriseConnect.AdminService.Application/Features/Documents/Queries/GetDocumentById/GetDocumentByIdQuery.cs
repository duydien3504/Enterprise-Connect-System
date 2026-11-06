using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Queries.GetDocumentById;

public class GetDocumentByIdQuery : IRequest<Result<DocumentDto>>
{
    public Guid Id { get; set; }

    public GetDocumentByIdQuery(Guid id)
    {
        Id = id;
    }
}
