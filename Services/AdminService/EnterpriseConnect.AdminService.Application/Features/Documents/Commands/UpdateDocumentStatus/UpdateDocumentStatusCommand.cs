using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UpdateDocumentStatus;

public class UpdateDocumentStatusCommand : IRequest<Result<DocumentDto>>
{
    public Guid Id { get; set; }
    public DocumentStatus Status { get; set; }
}
