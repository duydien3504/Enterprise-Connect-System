using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Mappings;
using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.AuditLogs.Queries.GetAuditLogById;

public class GetAuditLogByIdQueryHandler : IRequestHandler<GetAuditLogByIdQuery, Result<AuditLogDto>>
{
    private readonly IAdminUnitOfWork _unitOfWork;

    public GetAuditLogByIdQueryHandler(IAdminUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuditLogDto>> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
    {
        var log = await _unitOfWork.AuditLogs.GetByIdAsync(request.Id, cancellationToken);
        if (log == null)
        {
            return Result<AuditLogDto>.Failure($"Audit log with Id '{request.Id}' was not found.");
        }

        return Result<AuditLogDto>.Success(log.ToDto());
    }
}
