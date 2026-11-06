namespace EnterpriseConnect.AdminService.Application.Common.Models;

public class SystemSettingDto
{
    public Guid Id { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
}
