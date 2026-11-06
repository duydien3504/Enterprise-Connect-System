namespace EnterpriseConnect.AdminService.Api.DTOs;

public class UpdateSystemSettingRequest
{
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string GroupName { get; set; } = "General";
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
}
