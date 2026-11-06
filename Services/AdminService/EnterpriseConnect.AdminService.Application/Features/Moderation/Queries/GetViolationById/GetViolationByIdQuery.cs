using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Moderation.Queries.GetViolationById;

public class GetViolationByIdQuery : IRequest<Result<ToxicViolationDto>>
{
    public Guid Id { get; set; }

    public GetViolationByIdQuery(Guid id)
    {
        Id = id;
    }
}
