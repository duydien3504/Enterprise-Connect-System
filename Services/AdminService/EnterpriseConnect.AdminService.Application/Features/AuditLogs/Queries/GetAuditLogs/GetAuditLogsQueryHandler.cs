using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Mappings;
using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler 
    : IRequestHandler<GetAuditLogsQuery, Result<(List<AuditLogDto> Items, long TotalCount)>>
{
    private readonly IAdminUnitOfWork _unitOfWork;

    public GetAuditLogsQueryHandler(IAdminUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<(List<AuditLogDto> Items, long TotalCount)>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, total) = await _unitOfWork.AuditLogs.GetPagedAsync(
            request.CorrelationId,
            request.ServiceName,
            request.RiskLevel,
            request.From,
            request.To,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = items.Select(x => x.ToDto()).ToList();
        return Result<(List<AuditLogDto> Items, long TotalCount)>.Success((dtos, total));
    }
}
