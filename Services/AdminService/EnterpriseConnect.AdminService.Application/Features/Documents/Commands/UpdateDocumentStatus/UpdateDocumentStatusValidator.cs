using FluentValidation;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UpdateDocumentStatus;

public class UpdateDocumentStatusValidator : AbstractValidator<UpdateDocumentStatusCommand>
{
    public UpdateDocumentStatusValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
