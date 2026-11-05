using EnterpriseConnect.RemoteService.Domain.Enums;

namespace EnterpriseConnect.RemoteService.Application.Common.Models;

public class RemoteSessionDto
{
    public Guid Id { get; set; }
    public string SessionCode { get; set; } = string.Empty;
    public Guid RequesterId { get; set; }
    public Guid? SupporterId { get; set; }
    public RemoteSessionStatus Status { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? ConnectedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public int DurationSeconds { get; set; }
    public string? EndReason { get; set; }
    public List<AccessGrantDto> AccessGrants { get; set; } = new();
}
