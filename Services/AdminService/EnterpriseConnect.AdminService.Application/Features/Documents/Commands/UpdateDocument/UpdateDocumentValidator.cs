using FluentValidation;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Commands.UpdateDocument;

public class UpdateDocumentValidator : AbstractValidator<UpdateDocumentCommand>
{
    public UpdateDocumentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).MaximumLength(250).When(x => !string.IsNullOrEmpty(x.Title));
        RuleFor(x => x.Version).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Version));
    }
}
