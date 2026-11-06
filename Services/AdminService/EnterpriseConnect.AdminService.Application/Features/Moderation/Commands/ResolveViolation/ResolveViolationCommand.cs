using EnterpriseConnect.AdminService.Application.Common.Models;
using EnterpriseConnect.AdminService.Domain.Enums;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Moderation.Commands.ResolveViolation;

public class ResolveViolationCommand : IRequest<Result<ToxicViolationDto>>
{
    public Guid Id { get; set; }
    public ActionTaken ActionTaken { get; set; }
}
