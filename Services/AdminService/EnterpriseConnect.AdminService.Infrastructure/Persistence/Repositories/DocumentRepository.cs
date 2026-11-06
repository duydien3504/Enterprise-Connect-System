using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Domain.Entities;
using EnterpriseConnect.AdminService.Domain.Enums;
using EnterpriseConnect.AdminService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseConnect.AdminService.Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AdminDbContext _context;

    public DocumentRepository(AdminDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _context.Documents.AddAsync(document, cancellationToken);
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.AccessList)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<(List<Document> Items, long TotalCount)> GetPagedAsync(
        string? searchTerm,
        DocumentStatus? status,
        DocumentType? fileType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Documents.Include(d => d.AccessList).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(d => d.Title.Contains(searchTerm) || (d.Description != null && d.Description.Contains(searchTerm)));

        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);

        if (fileType.HasValue)
            query = query.Where(d => d.FileType == fileType.Value);

        var total = await query.LongCountAsync(cancellationToken);
        var items = await query.OrderByDescending(d => d.CreatedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        if (_context.Entry(document).State == EntityState.Detached)
        {
            _context.Documents.Attach(document);
            _context.Entry(document).State = EntityState.Modified;
        }

        foreach (var access in document.AccessList)
        {
            if (_context.Entry(access).State == EntityState.Detached || _context.Entry(access).State == EntityState.Modified)
            {
                _context.Entry(access).State = EntityState.Added;
            }
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Document document, CancellationToken cancellationToken = default)
    {
        _context.Documents.Remove(document);
        return Task.CompletedTask;
    }

    public async Task<long> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documents.LongCountAsync(cancellationToken);
    }

    public async Task<long> GetIndexedCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documents.LongCountAsync(d => d.Status == DocumentStatus.Indexed, cancellationToken);
    }
}
