namespace EnterpriseConnect.AdminService.Application.Common.Interfaces;

public interface IAdminUnitOfWork
{
    IAuditLogRepository AuditLogs { get; }
    IDocumentRepository Documents { get; }
    IToxicViolationRepository ToxicViolations { get; }
    ISystemSettingRepository SystemSettings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
