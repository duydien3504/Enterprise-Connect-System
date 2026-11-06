using EnterpriseConnect.AdminService.Application.Common.Interfaces;
using EnterpriseConnect.AdminService.Infrastructure.Persistence.Context;

namespace EnterpriseConnect.AdminService.Infrastructure.Persistence.Repositories;

public class AdminUnitOfWork : IAdminUnitOfWork
{
    private readonly AdminDbContext _context;

    public AdminUnitOfWork(
        AdminDbContext context,
        IAuditLogRepository auditLogs,
        IDocumentRepository documents,
        IToxicViolationRepository toxicViolations,
        ISystemSettingRepository systemSettings)
    {
        _context = context;
        AuditLogs = auditLogs;
        Documents = documents;
        ToxicViolations = toxicViolations;
        SystemSettings = systemSettings;
    }

    public IAuditLogRepository AuditLogs { get; }
    public IDocumentRepository Documents { get; }
    public IToxicViolationRepository ToxicViolations { get; }
    public ISystemSettingRepository SystemSettings { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
