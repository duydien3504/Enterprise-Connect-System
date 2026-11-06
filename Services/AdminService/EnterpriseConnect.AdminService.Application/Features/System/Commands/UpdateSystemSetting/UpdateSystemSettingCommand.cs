using EnterpriseConnect.AdminService.Application.Common.Models;
using MediatR;

namespace EnterpriseConnect.AdminService.Application.Features.System.Commands.UpdateSystemSetting;

public class UpdateSystemSettingCommand : IRequest<Result<SystemSettingDto>>
{
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string GroupName { get; set; } = "General";
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
}
