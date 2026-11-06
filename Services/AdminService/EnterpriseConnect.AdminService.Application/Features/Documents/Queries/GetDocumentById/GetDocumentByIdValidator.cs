using FluentValidation;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Queries.GetDocumentById;

public class GetDocumentByIdValidator : AbstractValidator<GetDocumentByIdQuery>
{
    public GetDocumentByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
