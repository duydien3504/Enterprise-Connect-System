namespace EnterpriseConnect.AdminService.Application.Common.Models;

public class SystemDashboardDto
{
    public long TotalAuditLogs { get; set; }
    public long SafeLogsCount { get; set; }
    public long WarningLogsCount { get; set; }
    public long DangerLogsCount { get; set; }
    public long TotalDocuments { get; set; }
    public long IndexedDocuments { get; set; }
    public long TotalViolations { get; set; }
    public long PendingViolations { get; set; }
    public List<string> ActiveServices { get; set; } = new();
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}
