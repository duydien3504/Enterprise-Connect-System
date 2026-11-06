using FluentValidation;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Queries.GetDocuments;

public class GetDocumentsValidator : AbstractValidator<GetDocumentsQuery>
{
    public GetDocumentsValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
