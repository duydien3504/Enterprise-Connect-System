using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Application.Common.Mappings;
using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.Documents.Queries.GetDocuments;

public class GetDocumentsQueryHandler 
    : IRequestHandler<GetDocumentsQuery, Result<(List<DocumentDto> Items, long TotalCount)>>
{
    private readonly IAdminUnitOfWork _unitOfWork;

    public GetDocumentsQueryHandler(IAdminUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<(List<DocumentDto> Items, long TotalCount)>> Handle(
        GetDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, total) = await _unitOfWork.Documents.GetPagedAsync(
            request.SearchTerm,
            request.Status,
            request.FileType,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = items.Select(x => x.ToDto()).ToList();
        return Result<(List<DocumentDto> Items, long TotalCount)>.Success((dtos, total));
    }
}
