using FluentValidation;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Commands.DeleteDocument;

public class DeleteDocumentValidator : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
