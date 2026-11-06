using EnterpriseConnect.AdminService.Domain.Entities;
using EnterpriseConnect.AdminService.Domain.Enums;

namespace EnterpriseConnect.AdminService.Application.Common.Interfaces;

public interface IDocumentRepository
{
    Task AddAsync(Document document, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<Document> Items, long TotalCount)> GetPagedAsync(
        string? searchTerm,
        DocumentStatus? status,
        DocumentType? fileType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);
    Task DeleteAsync(Document document, CancellationToken cancellationToken = default);
    Task<long> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<long> GetIndexedCountAsync(CancellationToken cancellationToken = default);
}
