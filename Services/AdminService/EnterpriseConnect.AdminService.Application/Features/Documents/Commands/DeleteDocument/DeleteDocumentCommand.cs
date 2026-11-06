using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Commands.DeleteDocument;

public class DeleteDocumentCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }

    public DeleteDocumentCommand(Guid id)
    {
        Id = id;
    }
}
